using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using CommonWindows.Helpers;
using PlanetGeneratorDll;

namespace PlanetCreator.WPF.Converters
{
    [ValueConversion(typeof(UColor), typeof(string))]
    public class ColorEditConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = (UColor)value;

            if (c.A == 0xff)
                return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");

            return "#" + c.ARGB.ToString("X8");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;

            if (string.IsNullOrEmpty(str))
                return Color.Black;

            str = str
                .Replace(" ", "")
                .Replace(".", "")
                .Replace("#", "");

            var bytes = DataHelper.DecodeHex(str);

            if (bytes.Length == 3)
                return new UColor(0xff, bytes[0], bytes[1], bytes[2]);

            var result = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            for (int i = 0; i < bytes.Length; i++)
                result[i] = bytes[i];

            return new UColor(result[0], result[1], result[2], result[3]);
        }

        #endregion
    }
}
