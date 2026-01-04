using System.Windows.Controls;
using Photobooth.App.ViewModels;

namespace Photobooth.App.Views;

public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
