using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PlanetCreator.WPF.Properties;
using PlanetGeneratorDll.Models;

namespace PlanetCreator.WPF.UI.ColorEditor
{
    public partial class ColorEditControl : UserControl, INotifyPropertyChanged
    {
        private readonly ShemaLayer __Layer;
        private readonly ColorHeight __ColorHeight;
        private int __H;

        public ColorHeight ColorHeight
        {
            get { return __ColorHeight; }
        }

        public int H
        {
            get { return __H; }
            set
            {
                if (__Layer.MaxHeight <= 0)
                    return;

                __H = value;

                var height = ColorEditor.IMAGE_HEIGHT;

                var percent = __H / (float)__Layer.MaxHeight;
                var top = height - percent * height;

                Margin = new Thickness(0, top - ActualHeight / 2, 0, 0);
            }
        }

        public ColorEditControl(ShemaLayer layer, ColorHeight height)
        {
            __Layer = layer;
            __ColorHeight = height;

            H = __ColorHeight.Heigth;

            InitializeComponent();
            DataContext = this;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            
        }

        private void Button_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
