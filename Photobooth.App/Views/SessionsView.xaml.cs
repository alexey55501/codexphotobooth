using System.Windows.Controls;
using Photobooth.App.ViewModels;

namespace Photobooth.App.Views;

public partial class SessionsView : UserControl
{
    public SessionsView(SessionsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
