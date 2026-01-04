using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Photobooth.Camera.Canon.Services;
using Photobooth.Domain.Models;
using Photobooth.Services.Configuration;
using Photobooth.Services.Export;
using Photobooth.Services.Sessions;

namespace Photobooth.App.ViewModels;

public class CaptureViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly ISessionService _sessionService;
    private readonly IExportService _exportService;
    private readonly ICameraService _cameraService;
    private AppSettings _settings = new();
    private SessionInfo? _currentSession;
    private string _countdownDisplay = string.Empty;
    private BitmapSource? _liveViewFrame;

    public CaptureViewModel(ISettingsService settingsService, ISessionService sessionService, IExportService exportService, ICameraService cameraService)
    {
        _settingsService = settingsService;
        _sessionService = sessionService;
        _exportService = exportService;
        _cameraService = cameraService;
        StartSessionCommand = new RelayCommand(async _ => await StartSession());
        CaptureCommand = new RelayCommand(async _ => await Capture());
        CompleteSessionCommand = new RelayCommand(async _ => await CompleteSession());
        _ = LoadSettings();
    }

    public ObservableCollection<string> CapturedPhotos { get; } = new();

    public ICommand StartSessionCommand { get; }
    public ICommand CaptureCommand { get; }
    public ICommand CompleteSessionCommand { get; }

    public string CountdownDisplay
    {
        get => _countdownDisplay;
        set
        {
            _countdownDisplay = value;
            OnPropertyChanged();
        }
    }

    public BitmapSource? LiveViewFrame
    {
        get => _liveViewFrame;
        set
        {
            _liveViewFrame = value;
            OnPropertyChanged();
        }
    }

    private async Task LoadSettings()
    {
        _settings = await _settingsService.LoadAsync();
    }

    private async Task StartSession()
    {
        _currentSession = await _sessionService.StartSessionAsync(_settings);
        CapturedPhotos.Clear();
        await EnsureCameraConnected();
    }

    private async Task Capture()
    {
        if (_currentSession == null)
        {
            await StartSession();
        }

        CountdownDisplay = string.Empty;
        for (int i = _settings.CountdownSeconds; i > 0; i--)
        {
            CountdownDisplay = i.ToString();
            await Task.Delay(1000);
        }
        CountdownDisplay = "";
        var path = Path.Combine(_sessionService.GetSessionFolder(_currentSession!), "originals", $"photo_{CapturedPhotos.Count + 1}.jpg");
        var image = await _cameraService.CapturePhotoAsync(path);
        if (image != null)
        {
            CapturedPhotos.Add(path);
        }
    }

    private async Task CompleteSession()
    {
        if (_currentSession != null)
        {
            await _sessionService.CompleteSessionAsync(_currentSession);
        }
    }

    private async Task EnsureCameraConnected()
    {
        await _cameraService.InitializeAsync();
        var cameras = await _cameraService.GetCamerasAsync();
        var firstCamera = cameras.FirstOrDefault();
        if (firstCamera == null)
        {
            throw new InvalidOperationException("No Canon cameras detected.");
        }

        if (!_cameraService.IsConnected)
        {
            await _cameraService.ConnectAsync(firstCamera);
        }
    }
}
