using System;
using System.Collections.Generic;
using System.Linq;
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

namespace BLiveSpotify_Plugin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public PluginDataContext context = new PluginDataContext();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = context;
        }

        private async void Login_btn_OnClick(object sender, RoutedEventArgs e)
        {
            await this.context.Plugin.spotifyLib.WaitLogin();
            context.OnPropertyChanged("LoginStatus");
            
        }

        private async void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            this.playlist_panel.IsEnabled = false;
            try
            {
                var r = await this.context.Plugin.spotifyLib.GetPlayDevices();
                this.playlists_electer.ItemsSource = r;
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
                this.playlist_panel.IsEnabled = true;
            }
        }
    }
}
