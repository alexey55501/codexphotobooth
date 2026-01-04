using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Photobooth.App.ViewModels;
using Photobooth.Camera.Canon.Services;
using Photobooth.Services.Configuration;
using Photobooth.Services.Export;
using Photobooth.Services.Printing;
using Photobooth.Services.Sessions;
using Photobooth.Services.Templates;
using Serilog;

namespace Photobooth.App;

public partial class App : Application
{
    public static IHost? Host { get; private set; }

    public App()
    {
        ConfigureHost();
    }

    private void ConfigureHost()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logPath = Path.Combine(appData, "Photobooth", "logs", "app.log");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<ISessionService, SessionService>();
                services.AddSingleton<IExportService, ExportService>();
                services.AddSingleton<ITemplateRenderingService, TemplateRenderingService>();
                services.AddSingleton<IPrintingService, PrintingService>();
                services.AddSingleton<ICameraService, CanonCameraService>();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainViewModel>();
            })
            .UseSerilog()
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await Host!.StartAsync();
        var window = Host.Services.GetRequiredService<MainWindow>();
        window.DataContext = Host.Services.GetRequiredService<MainViewModel>();
        window.Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (Host != null)
        {
            await Host.StopAsync();
            Host.Dispose();
            Host = null;
        }
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
