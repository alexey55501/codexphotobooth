using System.Text.Json;
using Photobooth.Domain.Constants;
using Photobooth.Domain.Models;

namespace Photobooth.Services.Sessions;

public interface ISessionService
{
    Task<SessionInfo> StartSessionAsync(AppSettings settings, DateTime? now = null);
    Task CompleteSessionAsync(SessionInfo session);
    string GetSessionFolder(SessionInfo session);
}

public class SessionService : ISessionService
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General) { WriteIndented = true };

    public async Task<SessionInfo> StartSessionAsync(AppSettings settings, DateTime? now = null)
    {
        var started = now ?? DateTime.UtcNow;
        var session = new SessionInfo { Started = started };
        var root = BuildSessionRoot(settings.OutputRoot, session);
        session.RootPath = root;
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, SessionFolders.Originals));
        Directory.CreateDirectory(Path.Combine(root, SessionFolders.Prints));
        Directory.CreateDirectory(Path.Combine(root, SessionFolders.Thumbs));
        Directory.CreateDirectory(Path.Combine(root, SessionFolders.Meta));
        await SaveMetadata(session);
        return session;
    }

    public async Task CompleteSessionAsync(SessionInfo session)
    {
        session.Completed = DateTime.UtcNow;
        await SaveMetadata(session);
    }

    public string GetSessionFolder(SessionInfo session)
    {
        return session.RootPath ?? BuildSessionRoot("Sessions", session);
    }

    private static string BuildSessionRoot(string root, SessionInfo session)
    {
        var date = session.Started.ToLocalTime().ToString("yyyy-MM-dd");
        return Path.Combine(root, date, session.Id);
    }

    private async Task SaveMetadata(SessionInfo session)
    {
        if (session.RootPath == null)
        {
            return;
        }

        var metaPath = Path.Combine(session.RootPath, SessionFolders.Meta, "session.json");
        await using var stream = File.Create(metaPath);
        await JsonSerializer.SerializeAsync(stream, session, _options);
    }
}
