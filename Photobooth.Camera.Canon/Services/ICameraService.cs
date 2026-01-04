using System.Windows.Media.Imaging;

namespace Photobooth.Camera.Canon.Services;

public interface ICameraService
{
    string Name { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CameraDeviceInfo>> GetCamerasAsync(CancellationToken cancellationToken = default);
    Task<bool> ConnectAsync(CameraDeviceInfo device, CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task<BitmapImage?> CapturePhotoAsync(string destinationPath, CancellationToken cancellationToken = default);
    Task<WriteableBitmap?> GetLiveViewFrameAsync(CancellationToken cancellationToken = default);
    bool IsConnected { get; }
}

public record CameraDeviceInfo(string Id, string DisplayName);
