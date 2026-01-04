using System.Text.Json;
using Photobooth.Domain.Models;

namespace Photobooth.Services.Configuration;

public interface ISettingsService
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
}

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
    {
        WriteIndented = true
    };

    public SettingsService(string? settingsPath = null)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsPath = settingsPath ?? Path.Combine(appData, "Photobooth", "settings.json");
    }

    public async Task<AppSettings> LoadAsync()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                await using var stream = File.OpenRead(_settingsPath);
                var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, _options);
                if (settings != null)
                {
                    return settings;
                }
            }
        }
        catch
        {
            // ignored; fall back to defaults
        }

        var defaults = new AppSettings();
        await SaveAsync(defaults);
        return defaults;
    }

    public async Task SaveAsync(AppSettings settings)
    {
        var dir = Path.GetDirectoryName(_settingsPath)!;
        Directory.CreateDirectory(dir);
        await using var stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, _options);
    }
}
