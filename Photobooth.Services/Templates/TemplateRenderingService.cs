using System.Windows.Media.Imaging;
using Photobooth.Template.Models;

namespace Photobooth.Services.Templates;

public interface ITemplateRenderingService
{
    BitmapSource Render(TemplateDocument template, IReadOnlyList<string?> photoPaths, Dictionary<string, string> tokens);
}

public class TemplateRenderingService : ITemplateRenderingService
{
    public BitmapSource Render(TemplateDocument template, IReadOnlyList<string?> photoPaths, Dictionary<string, string> tokens)
    {
        var pixelWidth = (int)Math.Round(template.WidthMm / 25.4 * template.Dpi);
        var pixelHeight = (int)Math.Round(template.HeightMm / 25.4 * template.Dpi);
        var bitmap = new WriteableBitmap(pixelWidth, pixelHeight, template.Dpi, template.Dpi, System.Windows.Media.PixelFormats.Pbgra32, null);
        bitmap.Lock();
        bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, pixelWidth, pixelHeight));
        bitmap.Unlock();
        return bitmap;
    }
}
