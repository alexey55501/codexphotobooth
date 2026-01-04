using System.Collections.ObjectModel;
using System.Text.Json;
using Photobooth.Domain.Models;

namespace Photobooth.App.ViewModels;

public class SessionsViewModel : ViewModelBase
{
    public ObservableCollection<SessionInfo> Sessions { get; } = new();

    public SessionsViewModel()
    {
        LoadSessions();
    }

    private void LoadSessions()
    {
        Sessions.Clear();
        var root = "Sessions";
        if (!Directory.Exists(root)) return;
        var files = Directory.GetFiles(root, "session.json", SearchOption.AllDirectories)
            .OrderByDescending(f => f)
            .Take(50);
        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            var session = JsonSerializer.Deserialize<SessionInfo>(json);
            if (session != null)
            {
                session.RootPath = Directory.GetParent(Directory.GetParent(file)!.FullName)!.FullName;
                Sessions.Add(session);
            }
        }
    }
}
