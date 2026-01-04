using Photobooth.Domain.Models;
using Photobooth.Services.Sessions;

namespace Photobooth.Tests.Sessions;

public class SessionServiceTests
{
    [Fact]
    public async Task CreatesFolderStructure()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var settings = new AppSettings { OutputRoot = tmp };
        var service = new SessionService();
        var session = await service.StartSessionAsync(settings, new DateTime(2024, 1, 1));
        Assert.True(Directory.Exists(Path.Combine(tmp, "2024-01-01", session.Id, "originals")));
        Assert.True(File.Exists(Path.Combine(tmp, "2024-01-01", session.Id, "meta", "session.json")));
    }
}
