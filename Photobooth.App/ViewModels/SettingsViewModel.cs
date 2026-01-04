using System.Collections.ObjectModel;
using System.Windows.Input;
using Photobooth.Domain.Models;
using Photobooth.Services.Configuration;
using Photobooth.Services.Printing;

namespace Photobooth.App.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly IPrintingService _printingService;
    private AppSettings _settings = new();

    public SettingsViewModel(ISettingsService settingsService, IPrintingService printingService)
    {
        _settingsService = settingsService;
        _printingService = printingService;
        Printers = new ObservableCollection<string>();
        SaveCommand = new RelayCommand(async _ => await Save());
        _ = Load();
    }

    public ObservableCollection<string> Printers { get; }

    public AppSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
            OnPropertyChanged();
        }
    }

    public ICommand SaveCommand { get; }

    private async Task Load()
    {
        Settings = await _settingsService.LoadAsync();
        foreach (var printer in await _printingService.GetPrinterNamesAsync())
        {
            Printers.Add(printer);
        }
    }

    private async Task Save()
    {
        await _settingsService.SaveAsync(Settings);
    }
}
