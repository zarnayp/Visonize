using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Visonize.UsImaging.Standalone.UI
{
    public class ThumbnailBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool selected && selected)
                return Brushes.LightGray;
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string enumValue = value.ToString();
            string targetValue = parameter.ToString();

            return enumValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Enum.Parse(targetType, parameter.ToString());

            return Binding.DoNothing;
        }
    }
}
