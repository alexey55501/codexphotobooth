using System.Text.Json.Serialization;

namespace Photobooth.Domain.Models;

public class SessionInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime Started { get; set; } = DateTime.UtcNow;
    public DateTime? Completed { get; set; }
    public List<SessionFile> Originals { get; set; } = new();
    public List<SessionFile> Prints { get; set; } = new();
    public List<SessionFile> Thumbnails { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();

    [JsonIgnore]
    public string? RootPath { get; set; }
}

public class SessionFile
{
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
