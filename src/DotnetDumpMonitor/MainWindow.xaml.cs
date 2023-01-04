using CliWrap;
using DotnetDumpMonitor.Commons;
using DotnetDumpMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace DotnetDumpMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
#if !DEBUG
            var githubLastReleaseVersion = await GithubUpgradeHelper.GetLastReleaseVersion();
            if (githubLastReleaseVersion > GithubUpgradeHelper.CurrentVersion)
            {
                MessageBox.Show($"Found new version({githubLastReleaseVersion})!!!");
                GithubUpgradeHelper.GoToDownloadPage();
            }
#endif
        }
    }
}
