using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private MainOverlay overlay;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = (-20);
        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);
        DanmakuLoader b = new BiliDMLib.DanmakuLoader();
        public MainWindow()
        {
            InitializeComponent();
            overlay = new MainOverlay();
            overlay.SourceInitialized += delegate
            {
                IntPtr hwnd = new WindowInteropHelper(overlay).Handle;
                uint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            };
            overlay.ShowInTaskbar = false;
            overlay.Top = SystemParameters.WorkArea.Top;
            overlay.Left = SystemParameters.WorkArea.Right - 250;
            overlay.Height = SystemParameters.WorkArea.Height;
            overlay.Width = 250;
            overlay.Show();
            
        }

        private async void connbtn_Click(object sender, RoutedEventArgs e)
        {
            
            int roomid = Convert.ToInt32(this.romid.Text.Trim());
            if (roomid > 0)
            {
                var connectresult = await b.ConnectAsync(roomid);
                if (connectresult)
                {
                    logging("連接成功");
                    AddDMText("彈幕姬報告", "連接成功", true);
                    this.connbtn.IsEnabled = false;
                    b.Disconnected += b_Disconnected;
                    b.ReceivedDanmaku += b_ReceivedDanmaku;
                }
                else
                {
                    logging("連接失敗");
                    AddDMText("彈幕姬報告", "連接失敗", true);
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
            AddDMText(e.Danmaku.CommentUser,e.Danmaku.CommentText);
        }

        void b_Disconnected(object sender, DisconnectEvtArgs args)
        {
            logging("連接被斷開:"+ args.Error);
            AddDMText("彈幕姬報告", "連接被斷開",true);
            this.connbtn.IsEnabled = true;
        }
        public void logging(string text)
        {
            if (log.Dispatcher.CheckAccess())
            {
                this.log.Text = this.log.Text + text + "\n";
                log.ScrollToEnd();
            }
            else
            {
                log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => logging(text)));
            }
        }

        public void AddDMText(string user,string text,bool warn=false)
        {
            if (overlay.Dispatcher.CheckAccess())
            {
                DanmakuTextControl c=new DanmakuTextControl();
                var sb = (Storyboard)c.Resources["Storyboard1"];
                sb.Completed += sb_Completed;
                c.UserName.Text = user;
                if (warn)
                {
                    c.UserName.Foreground=Brushes.Red;
                }
                c.Text.Text = text;
                c.ChangeHeight();
                overlay.LayoutRoot.Children.Add(c);
            }
            else
            {
                log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AddDMText(user,text)));
            }
        }

        void sb_Completed(object sender, EventArgs e)
        {
            var c = sender as DanmakuTextControl;
            if (c != null)
            {
                overlay.LayoutRoot.Children.Remove(c);
            }
        }

        private void Test_OnClick(object sender, RoutedEventArgs e)
        {
            AddDMText("彈幕姬報告", "這是一個測試", true);
        }
    }
}
