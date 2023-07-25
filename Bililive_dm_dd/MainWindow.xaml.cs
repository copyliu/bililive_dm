using System;
using System.Threading.Tasks;
using System.Windows;
using Bililive_dm_dd.Models;

namespace Bililive_dm_dd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            for (int i = 5050; i < 5060; i++)
            {
                Statics.Contexts.Add(new RoomContext(){RoomId = i});
            }
           
        }

        private async void ConnectBtn_Clicked(object sender, RoutedEventArgs e)
        {
            foreach (var roomContext in Statics.Contexts)
            {
                if (roomContext.Connected)continue;
                var _=roomContext.Connect();
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}