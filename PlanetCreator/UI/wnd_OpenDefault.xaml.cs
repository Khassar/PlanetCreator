using System.Windows;
using System.Windows.Input;
using PlanetGeneratorDll.Models;

namespace PlanetCreator.WPF.UI
{
    /// <summary>
    /// Interaction logic for wnd_OpenDefault.xaml
    /// </summary>
    public partial class wnd_OpenDefault
    {
        public PlanetContainer Result { get; private set; }

        public wnd_OpenDefault()
        {
            InitializeComponent();
            Loaded += wnd_OpenDefault_Loaded;
        }

        void wnd_OpenDefault_Loaded(object sender, RoutedEventArgs e)
        {
            var containers = PlanetContainer.GetDefaults();

            ListBoxContainer.ItemsSource = containers;
        }

        private void ListBoxContainer_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Result = ListBoxContainer.SelectedItem as PlanetContainer;
            if (Result != null)
                Close();
        }
    }
}
