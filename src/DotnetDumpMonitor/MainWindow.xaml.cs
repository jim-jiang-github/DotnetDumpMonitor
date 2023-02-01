using CliWrap;
using CommunityToolkit.Mvvm.Input;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DotnetDumpMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TopMostWindow _topMostWindow;
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            //_topMostWindow = new();
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            //_topMostWindow.Close();
        }

        private void SetTopButton_Click(object sender, RoutedEventArgs e)
        {
            //_topMostWindow.DataContext = ViewModel;
            //this.Hide();
            //_topMostWindow.Show();
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsHelper.SetUnTopMostWindow(hwnd);
            WindowsHelper.SetTopMostWindow(hwnd);
        }
    }
}
