using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CommonWindows.Convetors
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            var invert = false;
            var hidden = false;

            if (parameter != null)
            {
                var paramsStr = parameter.ToString();

                invert = paramsStr.Contains("invert");
                hidden = paramsStr.Contains("hidden");
            }

            var falseVisibility = hidden ?
                Visibility.Hidden :
                Visibility.Collapsed;

            var boolValue = System.Convert.ToBoolean(value);

            if (invert)
                boolValue = !boolValue;

            return boolValue ?
                Visibility.Visible : 
                falseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
