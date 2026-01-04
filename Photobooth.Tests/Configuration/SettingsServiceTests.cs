using Photobooth.Domain.Models;
using Photobooth.Services.Configuration;

namespace Photobooth.Tests.Configuration;

public class SettingsServiceTests
{
    [Fact]
    public async Task SavesAndLoadsSettings()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "settings.json");
        var service = new SettingsService(path);
        var settings = new AppSettings { OutputRoot = "Test" };
        await service.SaveAsync(settings);
        var loaded = await service.LoadAsync();
        Assert.Equal("Test", loaded.OutputRoot);
    }
}
