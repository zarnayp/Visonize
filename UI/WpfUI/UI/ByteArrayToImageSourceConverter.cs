using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Visonize.UsImaging.Application.ViewModels;

namespace Visonize.UsImaging.Standalone.UI
{
    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var thumbnail = (ThumbnailImage)value;

            var data = thumbnail.ImageData;

            try
            {
                if (data == null || data.Length == 0)
                    return null;

                if (thumbnail.ImageEncoding == ImageEncoding.CompressedJpeg)
                {
                    using var ms = new MemoryStream(data);

                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();

                    return bmp;
                }

                if (thumbnail.ImageEncoding == ImageEncoding.RawRgb)
                {
                    int stride = thumbnail.Width * 3;

                    return BitmapSource.Create(
                        thumbnail.Width,
                        thumbnail.Height,
                        96, 96,                      // DPI
                        PixelFormats.Rgb24,          // Pixel format
                        null,                        // No palette for RGB
                        data,                         // Raw pixel buffer
                        stride                       // Bytes per row
                        );
                }

            }
            catch
            {
                return null;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
