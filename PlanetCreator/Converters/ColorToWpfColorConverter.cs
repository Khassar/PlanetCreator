using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Common;

namespace PlanetCreator.WPF.Converters
{
    [ValueConversion(typeof(UColor), typeof(string))]
    public class ColorToWpfColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = (UColor)value;

            return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
