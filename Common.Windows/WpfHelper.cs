using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonWindows.Helpers;
using Microsoft.Win32;
using PlanetGeneratorDll;
using PlanetGeneratorDll.AMath;

namespace CommonWindows
{
    public class WpfHelper
    {
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
        public static BitmapSource ToBitmapSource(UBitmap uBitmap, APointF? size = null)
        {
            var bmp = DataHelper.ToBitmap(uBitmap);

            return ToBitmapSource(bmp, size);
        }

        public static BitmapSource ToBitmapSource(Bitmap bmp, APointF? size = null)
        {
            var ip = bmp.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = Imaging.CreateBitmapSourceFromHBitmap(ip,
                    IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            if (size.HasValue)
            {
                return new TransformedBitmap(bs,
                    new ScaleTransform(size.Value.X / bs.PixelWidth,
                        size.Value.Y / bs.PixelHeight));
            }

            return bs;
        }

        public static string ShowLoadFileDialog(string fileExtention, string description)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter =
                    description + " (*" + fileExtention + ")|*" + fileExtention +
                    "|All files (*.*)|*.*"
            };

            return openFileDialog.ShowDialog() == true ?
                openFileDialog.FileName :
                string.Empty;
        }

        public static string ShowSaveFileDialog(string fileExtention, string description)
        {
            var filter = string.IsNullOrEmpty(description) ? fileExtention.ToUpper() : description;
            var saveFileDialog = new SaveFileDialog
            {
                Filter = filter + " (*" + fileExtention + ")|*" + fileExtention + "|All files (*.*)|*.*",
                RestoreDirectory = true
            };

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }
    }
}
