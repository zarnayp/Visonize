using System.Windows;
using System.Windows.Controls;

namespace Standalone
{
    public partial class ViewportOverlay : UserControl
    {
        public event RoutedEventHandler? CloseClicked;

        public ViewportOverlay()
        {
            InitializeComponent();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseClicked?.Invoke(this, e);
        }
    }
}