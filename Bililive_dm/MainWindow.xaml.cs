using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using BiliDMLib;
namespace Bililive_dm
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void connbtn_Click(object sender, RoutedEventArgs e)
        {
            var b = new BiliDMLib.DanmakuLoader();
            int roomid = Convert.ToInt32(this.romid.Text.Trim());
            if (roomid > 0)
            {
                var connectresult = await b.ConnectAsync(roomid);
                if (connectresult)
                {
                    logging("連接成功");

                    b.Disconnected += b_Disconnected;
                    b.ReceivedDanmaku += b_ReceivedDanmaku;
                }
                else
                {
                    logging("連接失敗");
                }
            }
            else
            {
                MessageBox.Show("ID非法");
            }
        }

        void b_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            logging("收到彈幕:" + e.Danmaku.CommentUser+" 說: "+e.Danmaku.CommentText);
        }

        void b_Disconnected(object sender, DisconnectEvtArgs args)
        {
            logging("連接被斷開:"+ args.Error);
        }
        public void logging(string text)
        {
            if (log.Dispatcher.CheckAccess())
            {
                this.log.Text = this.log.Text + text + "\n";
            }
            else
            {
                log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => logging(text)));
            }
        }
    }
}
