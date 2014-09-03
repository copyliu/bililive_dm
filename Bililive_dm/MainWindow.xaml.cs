using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        private FullOverlay fulloverlay;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = (-20);
        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);
        DanmakuLoader b = new BiliDMLib.DanmakuLoader();
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
             timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, FuckMicrosoft,
                this.Dispatcher);
           timer.Start();
//            fulloverlay.Show();
            
        }

        private void FuckMicrosoft(object sender, EventArgs eventArgs)
        {
            if (fulloverlay != null)
            {
                fulloverlay.Topmost = false;
                fulloverlay.Topmost = true;
            }
            if (overlay != null)
            {
                overlay.Topmost = false;
                overlay.Topmost = true;
            }
        }

       

        private void OpenFullOverlay()
        {
            fulloverlay = new FullOverlay();
            fulloverlay.Deactivated += fulloverlay_Deactivated;
            fulloverlay.Background = Brushes.Transparent;
            fulloverlay.SourceInitialized += delegate
            {
                IntPtr hwnd = new WindowInteropHelper(fulloverlay).Handle;
                uint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            };
            fulloverlay.ShowInTaskbar = false;
            fulloverlay.Topmost = true;
            fulloverlay.Top = SystemParameters.WorkArea.Top;
            fulloverlay.Left = SystemParameters.WorkArea.Left;
            fulloverlay.Width = SystemParameters.WorkArea.Width;
            fulloverlay.Height = 550;
        }

        void fulloverlay_Deactivated(object sender, EventArgs e)
        {
            if (sender is FullOverlay)
            {
                (sender as FullOverlay).Topmost = true;
            }
        }

        private void OpenOverlay()
        {
            overlay = new MainOverlay();
            overlay.Deactivated += overlay_Deactivated;
            overlay.SourceInitialized += delegate
            {
                IntPtr hwnd = new WindowInteropHelper(overlay).Handle;
                uint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            };
            overlay.Background = Brushes.Transparent;
            overlay.ShowInTaskbar = false;
            overlay.Topmost = true;
            overlay.Top = SystemParameters.WorkArea.Top;
            overlay.Left = SystemParameters.WorkArea.Right - 250;
            overlay.Height = SystemParameters.WorkArea.Height;
            overlay.Width = 250;
        }

        void overlay_Deactivated(object sender, EventArgs e)
        {
            if (sender is MainOverlay)
            {
                (sender as MainOverlay).Topmost = true;
            }
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
                    b.ReceivedRoomCount += b_ReceivedRoomCount;
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

        void b_ReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
            logging("當前房間人數:" + e.UserCount);
            AddDMText("當前房間人數", e.UserCount+"", true);
            //AddDMText(e.Danmaku.CommentUser, e.Danmaku.CommentText);
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
                if (this.SideBar.IsChecked==true){
                DanmakuTextControl c=new DanmakuTextControl();

                c.UserName.Text = user;
                if (warn)
                {
                    c.UserName.Foreground=Brushes.Red;
                }
                c.Text.Text = text;
                c.ChangeHeight();
                var sb = (Storyboard)c.Resources["Storyboard1"];
                //Storyboard.SetTarget(sb,c);
                sb.Completed += sb_Completed;
                overlay.LayoutRoot.Children.Add(c);
                }
                if (this.Full.IsChecked == true)
                {
                   //<Storyboard x:Key="Storyboard1">
//			<ThicknessAnimationUsingKeyFrames Storyboard.TargetProperty="(FrameworkElement.Margin)" Storyboard.TargetName="fullScreenDanmaku">
//				<EasingThicknessKeyFrame KeyTime="0" Value="3,0,0,0"/>
//				<EasingThicknessKeyFrame KeyTime="0:0:1.9" Value="220,0,0,0"/>
//			</ThicknessAnimationUsingKeyFrames>
//		</Storyboard>
                    var v = new FullScreenDanmaku();
                    v.Text.Text = text;
                    v.ChangeHeight();
                    var wd = v.Text.DesiredSize.Width;
                    Dictionary<double,bool> dd=new Dictionary<double, bool>();
                    dd.Add(0, true);
                    foreach (var child in fulloverlay.LayoutRoot.Children)
                    {
                        if (child is FullScreenDanmaku)
                        {
                            var c = child as FullScreenDanmaku;
                            if (!dd.ContainsKey(c.Margin.Top))
                            {
                                dd.Add(c.Margin.Top,true);
                            }
                            if (c.Margin.Left > (SystemParameters.PrimaryScreenWidth - wd - 10))
                            {
                                dd[c.Margin.Top] = false;
                            }
                        }
                    }
                    double top;
                    if (dd.All(p => p.Value == false))
                    {
                        top = dd.Max(p => p.Key) + v.Text.DesiredSize.Height;
                    }
                    else
                    {
                        top = dd.Where(p=>p.Value).Min(p => p.Key) ;
                    }
                    Storyboard s=new Storyboard();
                    Duration duration = new Duration(TimeSpan.FromSeconds((SystemParameters.PrimaryScreenWidth+wd*2)/400));
                    ThicknessAnimation f = new ThicknessAnimation(new Thickness(SystemParameters.PrimaryScreenWidth, top, 0, 0), new Thickness(-wd, top, 0, 0), duration);
                    s.Children.Add(f);
                    s.Duration = duration;
                    Storyboard.SetTarget(f, v);
                    Storyboard.SetTargetProperty(f, new PropertyPath("(FrameworkElement.Margin)"));
                    fulloverlay.LayoutRoot.Children.Add(v);
                    s.Completed += s_Completed;
                    s.Begin();
                   


                }
            }
            else
            {
                log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AddDMText(user,text)));
            }
        }

        void s_Completed(object sender, EventArgs e)
        {
            var s = sender as AnimationClock;
            if (s == null) return;
            var c = Storyboard.GetTarget(s.Timeline) as FullScreenDanmaku;
            if (c != null)
            {
                fulloverlay.LayoutRoot.Children.Remove(c);
            }
        }

        void sb_Completed(object sender, EventArgs e)
        {
            var s = sender as ClockGroup;
            if (s == null) return;
            var c = Storyboard.GetTarget(s.Children[1].Timeline) as DanmakuTextControl;
            if (c != null)
            {
                overlay.LayoutRoot.Children.Remove(c);
            }
        }

        private void Test_OnClick(object sender, RoutedEventArgs e)
        {
            AddDMText("彈幕姬報告", "這是一個測試", true);
        }

        private void Full_Checked(object sender, RoutedEventArgs e)
        {
           
            //            overlay.Show();
            OpenFullOverlay();
                fulloverlay.Show();
           
        }

        private void SideBar_Checked(object sender, RoutedEventArgs e)
        {
            OpenOverlay();
                overlay.Show();
           
            
        }

        private void SideBar_Unchecked(object sender, RoutedEventArgs e)
        {
            overlay.Close();
        }

        private void Full_Unchecked(object sender, RoutedEventArgs e)
        {
            fulloverlay.Close();
        }
    }
}
