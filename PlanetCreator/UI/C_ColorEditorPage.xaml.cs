using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using PlanetCreator.WPF.Properties;
using PlanetGeneratorDll.Models;

namespace PlanetCreator.WPF.UI
{
    /// <summary>
    /// Interaction logic for C_ColorEditor.xaml
    /// </summary>
    public partial class C_ColorEditor : UserControl, INotifyPropertyChanged
    {
        private readonly MainWindow __Main;

        public ShemaLayer CurrentLayer
        {
            get { return ColorEditorControl.Layer; }
            set
            {
                ColorEditorControl.Layer = value;
                OnPropertyChanged();
            }
        }

        public List<ShemaLayer> Layers
        {
            get { return Shema.Layers; }
        }

        public Shema Shema
        {
            get { return __Main.Shema; }
            set
            {
                __Main.Shema = value;

                CurrentLayer = Layers[0];

                OnPropertyChanged();
                OnPropertyChanged("Layers");
            }
        }

        public C_ColorEditor()
        {
            __Main = MainWindow.Instance;
            __Main.SettingsChanged += Main_SettingsChanged;

            InitializeComponent();

            DataContext = this;
        }

        public void Main_SettingsChanged()
        {
            Shema = __Main.Shema;
            Update();
        }

        private void C_ColorEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentLayer = Shema.Layers[0];
        }

        private void OnLayerSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
                return;
            var layer = e.AddedItems[0] as ShemaLayer;
            if (layer != null)
                CurrentLayer = layer;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void UpdateLayer()
        {
            CurrentLayer = CurrentLayer;
        }

        private void GetRandom(object sender, RoutedEventArgs e)
        {
            Shema = Shema.GenerateRandom();
        }

        private void AddLayer_Click(object sender, RoutedEventArgs e)
        {
            Shema.Layers.Add(new ShemaLayer());

            Update();
        }

        private void RemoveLayer_Click(object sender, RoutedEventArgs e)
        {
            var ccl = ListViewLayers.SelectedItem as ShemaLayer;

            if (ccl == null)
                return;

            if (Shema.Layers.Count > 1)
                Shema.Layers.Remove(ccl);

            Update();
        }

        private void Update()
        {
            ListViewLayers.ItemsSource = null;
            ListViewLayers.ItemsSource = Layers;
        }
    }
}
