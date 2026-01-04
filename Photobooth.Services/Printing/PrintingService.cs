using System.Printing;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Photobooth.Services.Printing;

public interface IPrintingService
{
    Task<IEnumerable<string>> GetPrinterNamesAsync();
    Task PrintAsync(BitmapSource image, string? printerName = null, int copies = 1);
}

public class PrintingService : IPrintingService
{
    public Task<IEnumerable<string>> GetPrinterNamesAsync()
    {
        var servers = new LocalPrintServer();
        return Task.FromResult(servers.GetPrintQueues().Select(q => q.FullName).AsEnumerable());
    }

    public Task PrintAsync(BitmapSource image, string? printerName = null, int copies = 1)
    {
        var dialog = new PrintDialog();
        if (!string.IsNullOrEmpty(printerName))
        {
            dialog.PrintQueue = new PrintQueue(new PrintServer(), printerName);
        }
        dialog.PrintVisual(new Image { Source = image }, "Photobooth Print");
        return Task.CompletedTask;
    }
}
