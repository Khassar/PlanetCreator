using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using CommonWindows;

namespace PlanetCreator.WPF.UI
{
    /// <summary>
    /// Interaction logic for wnd_PlanetViewer.xaml
    /// </summary>
    public partial class wnd_PlanetViewer
    {
        private readonly Bitmap __Bitmap;

        public wnd_PlanetViewer(Bitmap bitmap)
        {
            __Bitmap = bitmap;

            InitializeComponent();
        }

        private void Wnd_PlanetViewer_OnLoaded(object sender, RoutedEventArgs e)
        {
            ImagePlanet.Source = WpfHelper.ToBitmapSource(__Bitmap);
        }

        private void SaveAsPng(object sender, RoutedEventArgs e)
        {
            var path = WpfHelper.ShowSaveFileDialog(".png", "png Image");

            if (!string.IsNullOrEmpty(path))
                __Bitmap.Save(path, ImageFormat.Png);
        }

        private void SaveAsJpg(object sender, RoutedEventArgs e)
        {
            var path = WpfHelper.ShowSaveFileDialog(".jpg", "jpeg Image");

            if (!string.IsNullOrEmpty(path))
                __Bitmap.Save(path, ImageFormat.Jpeg);
        }
    }
}
