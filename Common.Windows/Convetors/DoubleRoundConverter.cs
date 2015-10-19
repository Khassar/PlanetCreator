using System;
using System.Globalization;
using System.Windows.Data;

namespace CommonWindows.Convetors
{
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleRoundConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = (double)value;

            int p;


            if (!int.TryParse((string)parameter, out p))
                p = 1;

            return d.ToString("F" + p);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
