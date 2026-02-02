using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Visonize.UsImaging.Application.ViewModels;

namespace Visonize.UsImaging.Standalone
{
    /// <summary>
    /// Interaction logic for Thumbnails.xaml
    /// </summary>
    public partial class Thumbnails : UserControl
    {
        private ThumbnailsViewModel ThumbnailsViewModel => (ThumbnailsViewModel)DataContext;

        public Thumbnails()
        {
            InitializeComponent();
        }

        private void PART_Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = (Border)sender;

            var item = border.DataContext as Thumbnail;

            if (item != null)
            {
                if (item.Selected)
                {
                    (this.DataContext as ViewerViewModel)?.Thumbnails.UnselectImage(item.UsImage);
                }
                else
                {
                    (this.DataContext as ViewerViewModel)?.Thumbnails.SelectImage(item.UsImage);
                }
            }

        }
    }
}
