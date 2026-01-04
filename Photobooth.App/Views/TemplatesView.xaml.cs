using System.Windows.Controls;
using Photobooth.App.ViewModels;

namespace Photobooth.App.Views;

public partial class TemplatesView : UserControl
{
    public TemplatesView(TemplatesViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
