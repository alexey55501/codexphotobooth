using System.Windows.Controls;
using Photobooth.App.ViewModels;

namespace Photobooth.App.Views;

public partial class CaptureView : UserControl
{ 
    public CaptureView(CaptureViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
