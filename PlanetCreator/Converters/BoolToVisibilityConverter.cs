using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PlanetCreator.WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (bool)value;

            if (!string.IsNullOrEmpty(parameter as string) && parameter == "invert")
                b = !b;

            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;

            return int.Parse(str, NumberStyles.HexNumber);
        }

        #endregion
    }
}
