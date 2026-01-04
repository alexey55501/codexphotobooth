using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Photobooth.App.Views;
using Photobooth.Domain.Models;
using Photobooth.Services.Configuration;

namespace Photobooth.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;
    private readonly ISettingsService _settingsService;
    private object? _currentView;
    private AppSettings _settings = new();

    public MainViewModel(IServiceProvider services, ISettingsService settingsService)
    {
        _services = services;
        _settingsService = settingsService;
        NavigateCaptureCommand = new RelayCommand(_ => NavigateToCapture());
        NavigateTemplatesCommand = new RelayCommand(_ => NavigateToTemplates());
        NavigateSettingsCommand = new RelayCommand(_ => NavigateToSettings());
        NavigateSessionsCommand = new RelayCommand(_ => NavigateToSessions());
        _ = LoadSettings();
        NavigateToCapture();
    }

    public AppSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
            OnPropertyChanged();
        }
    }

    public object? CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }

    public ICommand NavigateCaptureCommand { get; }
    public ICommand NavigateTemplatesCommand { get; }
    public ICommand NavigateSettingsCommand { get; }
    public ICommand NavigateSessionsCommand { get; }

    private async Task LoadSettings()
    {
        Settings = await _settingsService.LoadAsync();
    }

    private void NavigateToCapture() => CurrentView = ActivatorUtilities.CreateInstance<CaptureView>(_services);
    private void NavigateToTemplates() => CurrentView = ActivatorUtilities.CreateInstance<TemplatesView>(_services);
    private void NavigateToSettings() => CurrentView = ActivatorUtilities.CreateInstance<SettingsView>(_services);
    private void NavigateToSessions() => CurrentView = ActivatorUtilities.CreateInstance<SessionsView>(_services);
}
