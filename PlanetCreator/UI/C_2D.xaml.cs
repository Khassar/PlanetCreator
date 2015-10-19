using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using PlanetCreator.WPF.Properties;
using PlanetGeneratorDll.Enums;
using PlanetGeneratorDll.Helpers;

namespace PlanetCreator.WPF.UI
{
    /// <summary>
    /// Interaction logic for C_GenerationEditor.xaml
    /// </summary>
    public partial class C_GenerationEditor : UserControl, INotifyPropertyChanged
    {
        #region private fields

        private readonly MainWindow __Main;

        #endregion

        #region public properties

        public double Seed
        {
            get { return __Main.Seed; }
            set
            {
                var seed = value;
                if (seed < -0.5)
                    seed = -0.5;
                else if (seed > 0.5)
                    seed = 0.5;

                __Main.Seed = seed;

                OnPropertyChanged();
            }
        }

        public double Lng
        {
            get { return __Main.Settings.Container2D.Lng; }
            set
            {
                __Main.Settings.Container2D.Lng = value;
                OnPropertyChanged();
            }
        }

        public double Lat
        {
            get { return __Main.Settings.Container2D.Lat; }
            set
            {
                __Main.Settings.Container2D.Lat = value;
                OnPropertyChanged();
            }
        }

        public ProjectionType Type
        {
            get
            {
                return (ProjectionType)Enum.Parse(typeof(ProjectionType), (string)ComboBoxProjection.SelectedItem);
            }
        }

        #endregion

        public C_GenerationEditor()
        {
            __Main = MainWindow.Instance;
            __Main.SettingsChanged += Main_SettingsChanged;

            InitializeComponent();

            DataContext = this;

            foreach (var type in (ProjectionType[])Enum.GetValues(typeof(ProjectionType)))
                ComboBoxProjection.Items.Add(type.ToString());

            ComboBoxProjection.SelectedIndex = 4;

            ComboBoxProjection.SelectionChanged += ComboBoxProjection_SelectionChanged;
        }

        void ComboBoxProjection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var type in (ProjectionType[]) Enum.GetValues(typeof (ProjectionType)))
            {
                if (type.ToString() == ComboBoxProjection.SelectedItem as string)
                {
                    __Main.Settings.Container2D.Projection = type;
                    return;
                }
            }
        }

        void Main_SettingsChanged()
        {
            Seed = Seed;
            Lng = Lng;
            Lat = Lat;
        }

        public void RandomSeeds()
        {
            Seed = RandomHelper.NextDouble(-0.5, 0.5, 9);
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

        private void RandomSeeds_Click(object sender, RoutedEventArgs e)
        {
            RandomSeeds();
        }
    }
}
