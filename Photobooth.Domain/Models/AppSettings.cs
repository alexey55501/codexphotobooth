namespace Photobooth.Domain.Models;

public class AppSettings
{
    public string OutputRoot { get; set; } = "Sessions";
    public string? SelectedPrinter { get; set; }
    public string? SelectedCamera { get; set; }
    public int CountdownSeconds { get; set; } = 3;
    public int ShotsPerSession { get; set; } = 1;
    public bool MockCamera { get; set; } = true;
    public int PrintCopies { get; set; } = 1;
    public bool KioskMode { get; set; }
    public string BoothName { get; set; } = "PhotoBooth";
}
