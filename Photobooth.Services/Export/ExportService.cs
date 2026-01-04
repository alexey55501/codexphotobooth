using Photobooth.Domain.Constants;
using Photobooth.Domain.Models;

namespace Photobooth.Services.Export;

public interface IExportService
{
    Task<string> SaveOriginalAsync(SessionInfo session, string fileName, Stream content);
    Task<string> SavePrintAsync(SessionInfo session, string fileName, Stream content);
    Task<string> SaveThumbnailAsync(SessionInfo session, string fileName, Stream content);
}

public class ExportService : IExportService
{
    public async Task<string> SaveOriginalAsync(SessionInfo session, string fileName, Stream content)
        => await SaveAsync(session, SessionFolders.Originals, fileName, content);

    public async Task<string> SavePrintAsync(SessionInfo session, string fileName, Stream content)
        => await SaveAsync(session, SessionFolders.Prints, fileName, content);

    public async Task<string> SaveThumbnailAsync(SessionInfo session, string fileName, Stream content)
        => await SaveAsync(session, SessionFolders.Thumbs, fileName, content);

    private static async Task<string> SaveAsync(SessionInfo session, string folder, string fileName, Stream content)
    {
        if (session.RootPath == null)
        {
            throw new InvalidOperationException("Session root missing");
        }

        var path = Path.Combine(session.RootPath, folder, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = File.Create(path);
        await content.CopyToAsync(file);
        var list = folder switch
        {
            SessionFolders.Originals => session.Originals,
            SessionFolders.Prints => session.Prints,
            SessionFolders.Thumbs => session.Thumbnails,
            _ => null
        };
        list?.Add(new SessionFile { FileName = fileName, Size = content.Length });
        return path;
    }
}
