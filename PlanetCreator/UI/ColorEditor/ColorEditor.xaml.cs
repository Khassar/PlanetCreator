using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using CommonWindows;
using PlanetGeneratorDll.AMath;
using PlanetGeneratorDll.Models;

namespace PlanetCreator.WPF.UI.ColorEditor
{
    /// <summary>
    /// Interaction logic for ColorEditor.xaml
    /// </summary>
    public partial class ColorEditor : UserControl, INotifyPropertyChanged
    {
        public const int IMAGE_HEIGHT = 450;

        private ShemaLayer __Layer;

        private List<ColorEditControl> __ColorEditControls;

        [Bindable(true)]
        public int ImageHeight
        {
            get { return IMAGE_HEIGHT; }
        }

        public ShemaLayer Layer
        {
            get { return __Layer; }
            set
            {
                CleareColorEditControls();

                __Layer = value;

                UpdateBitmap(null, null);
                OnPropertyChanged();
            }
        }

        public ColorEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UpdateControls()
        {
            if (__ColorEditControls == null)
                __ColorEditControls = new List<ColorEditControl>();

            var count = Layer.ColorHeights.Count;

            foreach (var control in __ColorEditControls)
            {
                control.Visibility = Visibility.Collapsed;
                control.IsEnabled = false;
            }

            __ColorEditControls.Clear();
            GridControls.Children.Clear();

            for (int i = 0; i < count; i++)
            {
                var h = Layer.ColorHeights[i];
                var control = new ColorEditControl(Layer, h);

                __ColorEditControls.Add(control);
                GridControls.Children.Add(control);
            }
        }

        private void CleareColorEditControls()
        {
            if (__ColorEditControls == null)
                return;

            foreach (var control in __ColorEditControls)
            {
                control.Visibility = Visibility.Collapsed;
                control.IsEnabled = false;
            }

            __ColorEditControls.Clear();
        }

        private void UpdateBitmap(object sender, RoutedEventArgs routedEventArgs)
        {
            ImageLayer.Source = WpfHelper.ToBitmapSource(__Layer.GenerateBitmap(),
                    new APointF((float)ImageLayer.Width, (float)ImageLayer.Height));

            UpdateControls();
        }

        private void AddColorHeigth(object sender, RoutedEventArgs e)
        {
            if (__Layer == null)
                return;

            Layer.AddNewHeigth();

            UpdateLayer();
        }

        private void UpdateLayer()
        {
            Layer = Layer;
        }

        private void RemoveSelectedColorHeigth(object sender, RoutedEventArgs e)
        {
            var ch = ListViewColorHeigths.SelectedItem as ColorHeight;

            if (ch == null)
                return;

            __Layer.ColorHeights.Remove(ch);

            if (__Layer.ColorHeights.Count > 0)
                ListViewColorHeigths.SelectedIndex = 0;

            UpdateLayer();

            UpdateBitmap(null, null);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
