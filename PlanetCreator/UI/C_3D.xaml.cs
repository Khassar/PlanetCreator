using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommonWindows;
using PlanetCreator.WPF.Properties;
using PlanetGeneratorDll;
using PlanetGeneratorDll.Enums;
using PlanetGeneratorDll.Models;

namespace PlanetCreator.WPF.UI
{
    /// <summary>
    /// Interaction logic for C_3D.xaml
    /// </summary>
    public partial class C_3D : UserControl, INotifyPropertyChanged
    {
        private readonly MainWindow __Main;

        public PlanetContainer3D Container
        {
            get { return __Main.Container3D; }
        }

        public int RecursionLevel
        {
            get { return Container.RecursionLevel; }
            set
            {
                Container.RecursionLevel = value;
                OnPropertyChanged();
            }
        }

        public float LandscapeOver
        {
            get { return Container.LandscapeOver; }
            set
            {
                Container.LandscapeOver = value;
                OnPropertyChanged();
            }
        }

        public float LandscapeUnder
        {
            get { return Container.LandscapeUnder; }
            set
            {
                Container.LandscapeUnder = value;
                OnPropertyChanged();
            }
        }

        public bool Optimize
        {
            get { return Container.Optimize; }
            set
            {
                Container.Optimize = value;
                OnPropertyChanged();
            }
        }

        public int OptimizePecent
        {
            get { return Container.OptimizePecent; }
            set
            {
                Container.OptimizePecent = value;
                OnPropertyChanged();
            }
        }

        public C_3D()
        {
            __Main = MainWindow.Instance;
            __Main.SettingsChanged += Main_SettingsChanged;

            InitializeComponent();

            DataContext = this;
        }

        void Main_SettingsChanged()
        {
            RecursionLevel = RecursionLevel;
            LandscapeOver = LandscapeOver;
            LandscapeUnder = LandscapeUnder;
            Optimize = Optimize;
            OptimizePecent = OptimizePecent;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ExportAsObj(object sender, RoutedEventArgs e)
        {
            Export(FileFormat3D.Obj);
        }

        private void ExportAsPly(object sender, RoutedEventArgs e)
        {
            Export(FileFormat3D.Ply);
        }

        private void ExportAsDae(object sender, RoutedEventArgs e)
        {
            Export(FileFormat3D.Dae);
        }

        private async void Export(FileFormat3D format)
        {
            var path = WpfHelper.ShowSaveFileDialog("." + format, format.ToString());
            if (string.IsNullOrEmpty(path))
                return;

            var controler = await __Main.MakeProgressDialog("Generating " + format);
            controler.SetCancelable(true);

            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var cancelTask = new Task(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (controler.IsCanceled)
                    {
                        cts.Cancel();
                        controler.CloseAsync();
                        break;
                    }

                    Thread.Sleep(100);
                }
            }, token);

            var generationTask = new Task(() =>
            {
                var main = MainWindow.Instance;

                var pg = new PlanetGenerator();
                var model = pg.Generate3D(main.Settings, null).Result;

                if (model == null)
                    return;

                using (var sw = new StreamWriter(path))
                {
                    switch (format)
                    {
                        case FileFormat3D.Ply:
                            model.ToPly(sw);
                            break;
                        case FileFormat3D.Dae:
                            model.ToDae(sw);
                            break;
                        case FileFormat3D.Obj:
                            model.ToObj(sw);
                            break;
                    }
                }
            });

            cancelTask.Start();

            generationTask.Start();

            await generationTask;

            cts.Cancel();
            await controler.CloseAsync();
        }

        private void UnityExtention_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
