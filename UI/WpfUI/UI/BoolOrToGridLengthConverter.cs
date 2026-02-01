using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Standalone
{
    // MultiValue converter: if any bound boolean is true -> Star, otherwise -> 0
    public class BoolOrToGridLengthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return new GridLength(0);

            bool any = values.OfType<bool>().Any(b => b);
            return any ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}