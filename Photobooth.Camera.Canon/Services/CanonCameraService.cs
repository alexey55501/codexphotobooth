using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;

namespace Photobooth.Camera.Canon.Services;

public class CanonCameraService : ICameraService
{
    private readonly ILogger<CanonCameraService> _logger;
    public CanonCameraService(ILogger<CanonCameraService> logger)
    {
        _logger = logger;
    }

    public string Name => "Canon EDSDK";

    public bool IsConnected { get; private set; }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing Canon SDK (placeholder)");
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<CameraDeviceInfo>> GetCamerasAsync(CancellationToken cancellationToken = default)
    {
        // In actual implementation, enumerate EDSDK devices.
        return Task.FromResult<IReadOnlyCollection<CameraDeviceInfo>>(Array.Empty<CameraDeviceInfo>());
    }

    public Task<bool> ConnectAsync(CameraDeviceInfo device, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Connecting to camera {Camera}", device.DisplayName);
        IsConnected = true;
        return Task.FromResult(true);
    }

    public Task DisconnectAsync()
    {
        _logger.LogInformation("Disconnecting camera");
        IsConnected = false;
        return Task.CompletedTask;
    }

    public Task<BitmapImage?> CapturePhotoAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Canon SDK capture not implemented in scaffold. Falling back to null image.");
        return Task.FromResult<BitmapImage?>(null);
    }

    public Task<WriteableBitmap?> GetLiveViewFrameAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<WriteableBitmap?>(null);
    }
}
