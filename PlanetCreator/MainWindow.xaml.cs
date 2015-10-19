using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommonWindows;
using CommonWindows.Helpers;
using MahApps.Metro.Controls.Dialogs;
using PlanetCreator.WPF.Properties;
using PlanetCreator.WPF.UI;
using PlanetGeneratorDll;
using PlanetGeneratorDll.Enums;
using PlanetGeneratorDll.Models;

namespace PlanetCreator.WPF
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly PlanetGenerator __PlanetGenerator;

        private bool __GenerationInProgress;
        private float __GenerationProgress;

        private bool __IsShowMode;
        private bool __Use4Threads;
        private bool __AlwaysRandomSeed;
        private PlanetContainer __Settings;

        private static MainWindow __Instance;
        public event Action SettingsChanged;

        #region public properties

        public static MainWindow Instance
        {
            get { return __Instance; }
        }

        public double Seed
        {
            get { return Settings.Seeds[0]; }
            set
            {
                Settings.Seeds[0] = value;
                OnPropertyChanged();
            }
        }

        public Shema Shema
        {
            get { return Settings.Shema; }
            set
            {
                Settings.Shema = value;
                OnPropertyChanged();
            }
        }

        public bool AlwaysRandomSeed
        {
            get { return __AlwaysRandomSeed; }
            set
            {
                __AlwaysRandomSeed = value;
                OnPropertyChanged();
            }
        }

        public PlanetContainer Settings
        {
            get { return __Settings; }
            set
            {
                __Settings = value;

                var handler = SettingsChanged;
                if (handler != null)
                    handler();

                OnPropertyChanged();
            }
        }

        public PlanetContainer2D Container2D
        {
            get { return Settings.Container2D; }
        }

        public PlanetContainer3D Container3D
        {
            get { return Settings.Container3D; }
        }

        public bool Use4Threads
        {
            get { return __Use4Threads; }
            set
            {
                __Use4Threads = value;
                OnPropertyChanged();
            }
        }

        public bool IsShowMode
        {
            get { return __IsShowMode; }
            set
            {
                __IsShowMode = value;
                OnPropertyChanged();
            }
        }

        public bool GenerationInProgress
        {
            get { return __GenerationInProgress; }
            set
            {
                __GenerationInProgress = value;

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    ProgressBarGeneration.Visibility = __GenerationInProgress
                        ? Visibility.Visible
                        : Visibility.Hidden));

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    ButtonGenerate.Visibility = __GenerationInProgress
                        ? Visibility.Collapsed
                        : Visibility.Visible));

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    ButtonAbort.Visibility = __GenerationInProgress
                        ? Visibility.Visible
                        : Visibility.Collapsed));
            }
        }

        public float GenerationProgress
        {
            get { return __GenerationProgress; }
            set
            {
                __GenerationProgress = value;

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    ProgressBarGeneration.Value = __GenerationProgress));
            }
        }

        #endregion

        public MainWindow()
        {
            Settings = new PlanetContainer();

            __Instance = this;

            InitializeComponent();

            DataContext = this;

            __PlanetGenerator = new PlanetGenerator();
        }

        public async Task<ProgressDialogController> MakeProgressDialog(string messageText)
        {
            return await this.ShowProgressAsync("Please wait...", messageText);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ProgressBarGeneration.Maximum = 100;
            GenerationInProgress = false;
            AlwaysRandomSeed = true;

            GenerationEditor.RandomSeeds();
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            Generate();
        }

        public void SetPlanetContainer(PlanetContainer2D colorContainer)
        {
            //ColorEditor.Shema = colorContainer.Shema;
            //GenerationEditor.PlanetContainer2D = colorContainer;
        }

        private Size GetSize()
        {
            int width;
            int heigth;

            if (IsShowMode)
            {
                int multi = ComboBoxSize.SelectedIndex;

                width = 1000 + 500 * multi;
                heigth = 1000 + 500 * multi;
            }
            else
            {
                width = (int)ImagePlanet.Width;
                heigth = (int)ImagePlanet.Height;
            }

            return new Size(width, heigth);
        }

        private async void Generate()
        {
            if (AlwaysRandomSeed)
                GenerationEditor.RandomSeeds();

            GenerationInProgress = true;

            var size = GetSize();

            Settings.Container2D.Width = (int)size.Width;
            Settings.Container2D.Height = (int)size.Height;

            var callBack = new Action<float>(percent => GenerationProgress = percent);

            var uBmp = await __PlanetGenerator.Generate2D(Settings, callBack, __Use4Threads);

            if (__PlanetGenerator.WasStoped)
                return;

            if (IsShowMode)
                new wnd_PlanetViewer(DataHelper.ToBitmap(uBmp)).Show();
            else
                ImagePlanet.Source = WpfHelper.ToBitmapSource(uBmp);

            GenerationInProgress = false;

            LabelLastGenTime.Content = "Last generation time : " + __PlanetGenerator.LastGenerationTime + "ms";
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

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            __PlanetGenerator.Abort();

            GenerationInProgress = false;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var filePath = WpfHelper.ShowLoadFileDialog(PlanetContainer.FILE_EXTENTION, PlanetContainer.FILE_DESCRIPTION);
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                Settings = JsonHelper.LoadFromFile<PlanetContainer>(filePath);
            }
            catch
            {
                this.ShowMessageAsync("Error", "Invalid file");
                Settings = new PlanetContainer();
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var fileName = WpfHelper.ShowSaveFileDialog(PlanetContainer.FILE_EXTENTION, PlanetContainer.FILE_DESCRIPTION);

            if (string.IsNullOrEmpty(fileName))
                return;

            JsonHelper.SaveToFile(Settings, fileName, true);
        }

        private void OpenDefault_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new wnd_OpenDefault();
            wnd.ShowDialog();

            var result = wnd.Result;
            if (result != null)
                Settings = result;
        }
    }
}
