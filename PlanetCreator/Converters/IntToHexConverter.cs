using System;
using System.Globalization;
using System.Windows.Data;

namespace PlanetCreator.WPF.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class IntToHexConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var i = (int)value;

            return i.ToString("X");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;

            return int.Parse(str, NumberStyles.HexNumber);
        }

        #endregion
    }
}
