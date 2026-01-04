using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;

namespace Photobooth.Camera.Canon.Services;

public class MockCameraService : ICameraService
{
    private readonly ILogger<MockCameraService> _logger;
    public MockCameraService(ILogger<MockCameraService> logger)
    {
        _logger = logger;
    }

    public string Name => "Mock Camera";

    public bool IsConnected { get; private set; }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<CameraDeviceInfo>> GetCamerasAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<CameraDeviceInfo>>(new[] { new CameraDeviceInfo("mock", "Mock Camera") });
    }

    public Task<bool> ConnectAsync(CameraDeviceInfo device, CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        return Task.FromResult(true);
    }

    public Task DisconnectAsync()
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    public Task<BitmapImage?> CapturePhotoAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        var bmp = new BitmapImage();
        return Task.FromResult<BitmapImage?>(bmp);
    }

    public Task<WriteableBitmap?> GetLiveViewFrameAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<WriteableBitmap?>(new WriteableBitmap(1, 1, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null));
    }
}
