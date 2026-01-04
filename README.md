# Photobooth

Offline-friendly WPF photobooth shell targeting .NET 8. Projects:
- Photobooth.App (WPF UI, MVVM navigation)
- Photobooth.Domain (core models)
- Photobooth.Services (settings, sessions, export, printing, rendering stubs)
- Photobooth.Camera.Canon (Canon SDK abstraction)
- Photobooth.Template (template model)
- Photobooth.Tests (unit tests)

## Building
Use Visual Studio 2022 with .NET 8 workloads on Windows. Open `Photobooth.sln` and build. On non-Windows hosts enable Windows targeting in CLI builds.

## Canon SDK
Place Canon EDSDK binaries in a folder referenced by the Canon project and ensure they copy to output. The camera service uses P/Invoke against `EDSDK.dll`; provide the native binaries alongside the application output.

## Running
Launch `Photobooth.App`. The shell provides Capture, Templates, Settings, and Sessions navigation. Settings persist to `%APPDATA%/Photobooth/settings.json`.

## Sessions
Session folders default to `Sessions/YYYY-MM-DD/SESSION_ID/{originals,prints,thumbs,meta}` with `session.json` metadata.

## Testing
Run `dotnet test` on Windows with .NET 8 SDK installed.
