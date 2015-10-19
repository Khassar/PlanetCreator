using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PlanetCreator.WPF.UI
{
    /// <summary>
    /// Interaction logic for C_About.xaml
    /// </summary>
    public partial class C_About : UserControl
    {
        public C_About()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void C_About_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxChangeLog.Text = Properties.Resources.ChangeLog;
        }
    }
}
