using System;
using System.Windows;

namespace BLiveSpotify_Plugin
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public PluginDataContext context = new PluginDataContext();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = context;
        }

        private async void Login_btn_OnClick(object sender, RoutedEventArgs e)
        {
            await context.Plugin.spotifyLib.WaitLogin();
            context.OnPropertyChanged("LoginStatus");
        }

        private async void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            playlist_panel.IsEnabled = false;
            try
            {
                var r = await context.Plugin.spotifyLib.GetPlayDevices();
                playlists_electer.ItemsSource = r;
                context.OnPropertyChanged("LoginStatus");
            }
            catch (MyException ex)
            {
                MessageBox.Show(this, ex.Message);
            }
            catch (Exception exception)
            {
            }
            finally
            {
                playlist_panel.IsEnabled = true;
            }
        }
    }
}