using System;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Photobooth.Camera.Canon.Services;

public class CanonCameraService : ICameraService, IDisposable
{
    private readonly ILogger<CanonCameraService> _logger;
    private readonly List<DiscoveredCamera> _discovered = new();
    private IntPtr _cameraList = IntPtr.Zero;
    private IntPtr _cameraRef = IntPtr.Zero;
    private bool _initialized;
    private TaskCompletionSource<string>? _captureCompletion;
    private string? _pendingCapturePath;
    private EdsdkNative.EdsObjectEventHandler? _objectHandler;

    private record DiscoveredCamera(CameraDeviceInfo Info, int Index);

    public CanonCameraService(ILogger<CanonCameraService> logger)
    {
        _logger = logger;
    }

    public string Name => "Canon EDSDK";

    public bool IsConnected { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await Task.Run(() =>
        {
            var result = EdsdkNative.EdsInitializeSDK();
            if (result != EdsdkNative.EDS_ERR_OK)
            {
                throw new InvalidOperationException($"Failed to initialize EDSDK: 0x{result:X}");
            }
            _initialized = true;
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<CameraDeviceInfo>> GetCamerasAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitialized(cancellationToken).ConfigureAwait(false);

        _discovered.Clear();
        await Task.Run(() =>
        {
            ReleaseCameraList();
            var listResult = EdsdkNative.EdsGetCameraList(out _cameraList);
            if (listResult != EdsdkNative.EDS_ERR_OK)
            {
                throw new InvalidOperationException($"Unable to enumerate cameras: 0x{listResult:X}");
            }

            var countResult = EdsdkNative.EdsGetChildCount(_cameraList, out int count);
            if (countResult != EdsdkNative.EDS_ERR_OK)
            {
                throw new InvalidOperationException($"Unable to read camera count: 0x{countResult:X}");
            }

            for (int i = 0; i < count; i++)
            {
                var childResult = EdsdkNative.EdsGetChildAtIndex(_cameraList, i, out var cameraRef);
                if (childResult != EdsdkNative.EDS_ERR_OK)
                {
                    _logger.LogWarning("Skipping camera index {Index} due to error 0x{Error:X}", i, childResult);
                    continue;
                }

                try
                {
                    var deviceInfoResult = EdsdkNative.EdsGetDeviceInfo(cameraRef, out var deviceInfo);
                    if (deviceInfoResult != EdsdkNative.EDS_ERR_OK)
                    {
                        _logger.LogWarning("Failed to read device info for index {Index}: 0x{Error:X}", i, deviceInfoResult);
                        continue;
                    }

                    var info = new CameraDeviceInfo($"{deviceInfo.szPortName}-{i}", deviceInfo.szDeviceDescription);
                    _discovered.Add(new DiscoveredCamera(info, i));
                }
                finally
                {
                    EdsdkNative.EdsRelease(cameraRef);
                }
            }
        }, cancellationToken).ConfigureAwait(false);

        return _discovered.Select(d => d.Info).ToArray();
    }

    public async Task<bool> ConnectAsync(CameraDeviceInfo device, CancellationToken cancellationToken = default)
    {
        await EnsureInitialized(cancellationToken).ConfigureAwait(false);

        var target = _discovered.FirstOrDefault(c => c.Info.Id == device.Id);
        if (target == null)
        {
            throw new InvalidOperationException($"Camera {device.DisplayName} was not discovered. Call GetCamerasAsync first.");
        }

        return await Task.Run(() =>
        {
            var cameraResult = EdsdkNative.EdsGetChildAtIndex(_cameraList, target.Index, out _cameraRef);
            if (cameraResult != EdsdkNative.EDS_ERR_OK)
            {
                throw new InvalidOperationException($"Failed to get camera reference: 0x{cameraResult:X}");
            }

            var sessionResult = EdsdkNative.EdsOpenSession(_cameraRef);
            if (sessionResult != EdsdkNative.EDS_ERR_OK)
            {
                EdsdkNative.EdsRelease(_cameraRef);
                _cameraRef = IntPtr.Zero;
                throw new InvalidOperationException($"Failed to open camera session: 0x{sessionResult:X}");
            }

            var saveToHost = Marshal.AllocHGlobal(sizeof(int));
            try
            {
                Marshal.WriteInt32(saveToHost, EdsdkNative.kEdsSaveTo_Host);
                var saveResult = EdsdkNative.EdsSetPropertyData(_cameraRef, EdsdkNative.kEdsPropertyID_SaveTo, 0, sizeof(int), saveToHost);
                if (saveResult != EdsdkNative.EDS_ERR_OK)
                {
                    _logger.LogWarning("Unable to configure SaveTo host (0x{Error:X})", saveResult);
                }

                var capacity = new EdsdkNative.EdsCapacity
                {
                    BytesPerSector = 0x1000,
                    NumberOfFreeClusters = 0x7FFFFFFF,
                    Reset = 1
                };
                var capacityResult = EdsdkNative.EdsSetCapacity(_cameraRef, capacity);
                if (capacityResult != EdsdkNative.EDS_ERR_OK)
                {
                    _logger.LogWarning("Unable to set camera capacity (0x{Error:X})", capacityResult);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(saveToHost);
            }

            _objectHandler = OnObjectEvent;
            var handlerResult = EdsdkNative.EdsSetObjectEventHandler(_cameraRef, EdsdkNative.kEdsObjectEvent_DirItemCreated, _objectHandler, IntPtr.Zero);
            if (handlerResult != EdsdkNative.EDS_ERR_OK)
            {
                _logger.LogWarning("Failed to register object event handler: 0x{Error:X}", handlerResult);
            }

            IsConnected = true;
            return true;
        }, cancellationToken).ConfigureAwait(false);
    }

    public Task DisconnectAsync()
    {
        if (_cameraRef != IntPtr.Zero)
        {
            EdsdkNative.EdsCloseSession(_cameraRef);
            EdsdkNative.EdsRelease(_cameraRef);
            _cameraRef = IntPtr.Zero;
        }
        IsConnected = false;
        return Task.CompletedTask;
    }

    public async Task<BitmapImage?> CapturePhotoAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        if (!IsConnected || _cameraRef == IntPtr.Zero)
        {
            throw new InvalidOperationException("Camera not connected.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        _pendingCapturePath = destinationPath;
        _captureCompletion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        await Task.Run(() =>
        {
            var result = EdsdkNative.EdsSendCommand(_cameraRef, EdsdkNative.kEdsCameraCommand_TakePicture, 0);
            if (result != EdsdkNative.EDS_ERR_OK)
            {
                throw new InvalidOperationException($"Failed to trigger capture: 0x{result:X}");
            }
        }, cancellationToken).ConfigureAwait(false);

        var filePath = await _captureCompletion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(filePath);
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    public Task<WriteableBitmap?> GetLiveViewFrameAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Live view streaming requires EVF support and is not implemented in this revision.");
        return Task.FromResult<WriteableBitmap?>(null);
    }

    private int OnObjectEvent(uint inEvent, IntPtr inRef, IntPtr inContext)
    {
        if (_pendingCapturePath == null)
        {
            return EdsdkNative.EDS_ERR_OK;
        }

        if (inEvent == EdsdkNative.kEdsObjectEvent_DirItemCreated || inEvent == EdsdkNative.kEdsObjectEvent_DirItemRequestTransfer)
        {
            try
            {
                DownloadImage(inRef, _pendingCapturePath);
                _captureCompletion?.TrySetResult(_pendingCapturePath);
            }
            catch (Exception ex)
            {
                _captureCompletion?.TrySetException(ex);
            }
        }

        return EdsdkNative.EDS_ERR_OK;
    }

    private void DownloadImage(IntPtr dirItemRef, string destinationPath)
    {
        var streamResult = EdsdkNative.EdsCreateFileStream(destinationPath, EdsdkNative.kEdsFileCreateDisposition_CreateAlways, EdsdkNative.kEdsAccess_ReadWrite, out var stream);
        if (streamResult != EdsdkNative.EDS_ERR_OK)
        {
            throw new InvalidOperationException($"Failed to create file stream: 0x{streamResult:X}");
        }

        try
        {
            var downloadResult = EdsdkNative.EdsDownload(dirItemRef, uint.MaxValue, stream);
            if (downloadResult != EdsdkNative.EDS_ERR_OK)
            {
                throw new InvalidOperationException($"Download failed: 0x{downloadResult:X}");
            }

            var completeResult = EdsdkNative.EdsDownloadComplete(dirItemRef);
            if (completeResult != EdsdkNative.EDS_ERR_OK)
            {
                _logger.LogWarning("Download complete reported error 0x{Error:X}", completeResult);
            }
        }
        finally
        {
            EdsdkNative.EdsRelease(stream);
            EdsdkNative.EdsRelease(dirItemRef);
        }
    }

    private Task EnsureInitialized(CancellationToken cancellationToken)
    {
        return _initialized ? Task.CompletedTask : InitializeAsync(cancellationToken);
    }

    private void ReleaseCameraList()
    {
        if (_cameraList != IntPtr.Zero)
        {
            EdsdkNative.EdsRelease(_cameraList);
            _cameraList = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        ReleaseCameraList();
        if (_initialized)
        {
            EdsdkNative.EdsTerminateSDK();
            _initialized = false;
        }
    }
}
