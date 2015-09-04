using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using BiliDMLib;
using Bililive_dm.Annotations;

namespace Bililive_dm
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainOverlay overlay;
        public FullOverlay fulloverlay;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = (-20);

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        private DanmakuLoader b = new BiliDMLib.DanmakuLoader();
        private DispatcherTimer timer;
        private const int _maxCapacity = 100;

        private Queue<string> _messageQueue = new Queue<string>(_maxCapacity);
        private ObservableCollection<SessionItem> SessionItems=new ObservableCollection<SessionItem>();
        public MainWindow()
        {
            InitializeComponent();
            web.Source=new Uri("http://soft.ceve-market.org/bilibili_dm/app.htm?"+DateTime.Now.Ticks); //fuck you IE cache
            b.Disconnected += b_Disconnected;
            b.ReceivedDanmaku += b_ReceivedDanmaku;
            b.ReceivedRoomCount += b_ReceivedRoomCount;
            StoreModel settings;
            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                                            IsolatedStorageScope.Domain |
                                                                            IsolatedStorageScope.Assembly, null, null);
                System.Xml.Serialization.XmlSerializer settingsreader =
                    new System.Xml.Serialization.XmlSerializer(typeof (StoreModel));
                StreamReader reader = new StreamReader(new IsolatedStorageFileStream(
                    "settings.xml", FileMode.Open, isoStore));
                settings = (StoreModel) settingsreader.Deserialize(reader);
                
            }
            catch (Exception)
            {
                settings=new StoreModel();
                
            }
            settings.SaveConfig();
            settings.toStatic();
            OptionDialog.LayoutRoot.DataContext = settings;

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, FuckMicrosoft,
                this.Dispatcher);
            timer.Start();
            
            DataGrid.ItemsSource = Ranking;
            DataGrid2.ItemsSource = SessionItems;
//            fulloverlay.Show();
            logging("投喂记录不会在弹幕模式上出现, 这不是bug");
//            for (int i = 0; i < 150; i++)
//            {
//                logging("投喂记录不会在弹幕模式上出现, 这不是bug");
//            }
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

        private void fulloverlay_Deactivated(object sender, EventArgs e)
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
            overlay.Top = SystemParameters.WorkArea.Top + Store.MainOverlayXoffset;
            overlay.Left = SystemParameters.WorkArea.Right - Store.MainOverlayWidth + Store.MainOverlayYoffset;
            overlay.Height = SystemParameters.WorkArea.Height;
            overlay.Width = Store.MainOverlayWidth;
        }

        private void overlay_Deactivated(object sender, EventArgs e)
        {
            if (sender is MainOverlay)
            {
                (sender as MainOverlay).Topmost = true;
            }
        }

        private async void connbtn_Click(object sender, RoutedEventArgs e)
        {
            int roomid;
            try
            {
                roomid = Convert.ToInt32(this.romid.Text.Trim());

            }
            catch (Exception)
            {
                MessageBox.Show("请输入房间号,房间号是!数!字!");
                return;
            }
             
            if (roomid > 0)
            {
                var connectresult = false;
                logging("正在连接");
                connectresult = await b.ConnectAsync(roomid);
                while (!connectresult && sender==null && AutoReconnect.IsChecked==true)
                {
                    logging("正在连接");
                    connectresult = await b.ConnectAsync(roomid);
                    
                }
                
                
                if (connectresult)
                {
                    logging("連接成功");
                    AddDMText("彈幕姬報告", "連接成功", true);
                    Ranking.Clear();
                    this.connbtn.IsEnabled = false;
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

        private void b_ReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
//            logging("當前房間人數:" + e.UserCount);
//            AddDMText("當前房間人數", e.UserCount+"", true);
            //AddDMText(e.Danmaku.CommentUser, e.Danmaku.CommentText);
            if (this.CheckAccess())
            {
                OnlineBlock.Text = e.UserCount + "";
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action(() => OnlineBlock.Text = e.UserCount + ""));
            }
        }

        private ObservableCollection<BiliDMLib.GiftRank> Ranking = new ObservableCollection<GiftRank>();

        private void b_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            switch (e.Danmaku.MsgType)
            {
                case MsgTypeEnum.Comment:
                    logging("收到彈幕:" + (e.Danmaku.isAdmin?"[管]":"")+ (e.Danmaku.isVIP ? "[爷]" : "") +e.Danmaku.CommentUser + " 說: " + e.Danmaku.CommentText);

                    AddDMText((e.Danmaku.isAdmin ? "[管]" : "") + (e.Danmaku.isVIP ? "[爷]" : "") + e.Danmaku.CommentUser, e.Danmaku.CommentText);
                    break;
                case MsgTypeEnum.GiftTop:
                    foreach (var giftRank in e.Danmaku.GiftRanking)
                    {
                        var query = Ranking.Where(p => p.uid == giftRank.uid);
                        if (query.Any())
                        {
                            var f = query.First();
                            this.Dispatcher.BeginInvoke(new Action(() => f.coin = giftRank.coin));

                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(new Action(() => Ranking.Add(new GiftRank()
                            {
                                uid = giftRank.uid,
                                coin = giftRank.coin,
                                UserName = giftRank.UserName
                            })));

                        }
                    }
                    break;
                case MsgTypeEnum.GiftSend:
                {
                    var query = SessionItems.Where(p => p.UserName == e.Danmaku.GiftUser && p.Item == e.Danmaku.GiftName);
                    if (query.Any())
                    {
                        this.Dispatcher.BeginInvoke(
                            new Action(() => query.First().num += Convert.ToDecimal(e.Danmaku.GiftNum)));

                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => SessionItems.Add(
                            new SessionItem()
                            {
                                Item = e.Danmaku.GiftName,
                                UserName = e.Danmaku.GiftUser,
                                num = Convert.ToDecimal(e.Danmaku.GiftNum)
                            }
                            )));

                    }
                    logging("收到道具:" + e.Danmaku.GiftUser + " 赠送的: " + e.Danmaku.GiftName + " x " + e.Danmaku.GiftNum);
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ShowItem.IsChecked == true)
                        {
                            AddDMText("收到道具",
                                e.Danmaku.GiftUser + " 赠送的: " + e.Danmaku.GiftName + " x " + e.Danmaku.GiftNum, true);
                        }
                    }));
                    break;
                }
                case MsgTypeEnum.Welcome:
                {
                        logging("欢迎老爷"+(e.Danmaku.isAdmin?"和管理":"")+": " + e.Danmaku.CommentUser + " 进入直播间");
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (ShowItem.IsChecked == true)
                            {
                                AddDMText("欢迎老爷" + (e.Danmaku.isAdmin ? "和管理" : ""),
                                    e.Danmaku.CommentUser + " 进入直播间", true);
                            }
                        }));
                        
                        break;
                    }



            }

        }

        private void b_Disconnected(object sender, DisconnectEvtArgs args)
        {
            logging("連接被斷開: 开发者信息" + args.Error);
            AddDMText("彈幕姬報告", "連接被斷開", true);

            if (this.CheckAccess())
            {
                if (AutoReconnect.IsChecked == true && args.Error != null)
                {
                    logging("正在自动重连...");
                    AddDMText("彈幕姬報告", "正在自动重连", true);
                    connbtn_Click(null, null);
                }
                else
                {
                    this.connbtn.IsEnabled = true;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (AutoReconnect.IsChecked == true && args.Error != null)
                    {
                        logging("正在自动重连...");
                        AddDMText("彈幕姬報告", "正在自动重连", true);
                        connbtn_Click(null, null);
                    }
                    else
                    {
                        this.connbtn.IsEnabled = true;
                    }
                }));
            }
        }

        public void logging(string text)
        {


            if (log.Dispatcher.CheckAccess())
            {
                
                if (_messageQueue.Count >= _maxCapacity)
                {
                    _messageQueue.Dequeue();
                }

                _messageQueue.Enqueue(DateTime.Now.ToString("T")+" : " +text);
                this.log.Text = string.Join("\n", _messageQueue);
                log.CaretIndex = this.log.Text.Length;
                log.ScrollToEnd();

                if (this.SaveLog.IsChecked == true) { 
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


                    path = System.IO.Path.Combine(path, "弹幕姬");
                    System.IO.Directory.CreateDirectory(path);
                    using (StreamWriter outfile = new StreamWriter(System.IO.Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd") + ".txt"),true))
                    {
                        outfile.WriteLine(DateTime.Now.ToString("T") + " : " + text);
                    }
                }
                catch (Exception ex)
                {
                }
                }

            }
            else
            {
                log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => logging(text)));
            }
        }

        public void AddDMText(string user, string text, bool warn = false,bool foreceenablefullscreen=false)
        {
            if (overlay.Dispatcher.CheckAccess())
            {
                if (this.SideBar.IsChecked == true)
                {
                    DanmakuTextControl c = new DanmakuTextControl();

                    c.UserName.Text = user;
                    if (warn)
                    {
                        c.UserName.Foreground = Brushes.Red;
                    }
                    c.Text.Text = text;
                    c.ChangeHeight();
                    var sb = (Storyboard) c.Resources["Storyboard1"];
                    //Storyboard.SetTarget(sb,c);
                    sb.Completed += sb_Completed;
                    overlay.LayoutRoot.Children.Add(c);
                }
                if (this.Full.IsChecked == true && (!warn || foreceenablefullscreen))
                {
                    //<Storyboard x:Key="Storyboard1">
//			<ThicknessAnimationUsingKeyFrames Storyboard.TargetProperty="(FrameworkElement.Margin)" Storyboard.TargetName="fullScreenDanmaku">
//				<EasingThicknessKeyFrame KeyTime="0" Value="3,0,0,0"/>
//				<EasingThicknessKeyFrame KeyTime="0:0:1.9" Value="220,0,0,0"/>
//			</ThicknessAnimationUsingKeyFrames>
//		</Storyboard>
                    lock (fulloverlay.LayoutRoot.Children)
                    {
                        var v = new FullScreenDanmaku();
                        v.Text.Text = text;
                        v.ChangeHeight();
                        var wd = v.Text.DesiredSize.Width;

                        Dictionary<double, bool> dd = new Dictionary<double, bool>();
                        dd.Add(0, true);
                        foreach (var child in fulloverlay.LayoutRoot.Children)
                        {
                            if (child is FullScreenDanmaku)
                            {
                                var c = child as FullScreenDanmaku;
                                if (!dd.ContainsKey(Convert.ToInt32(c.Margin.Top)))
                                {
                                    dd.Add(Convert.ToInt32(c.Margin.Top), true);
                                }
                                if (c.Margin.Left > (SystemParameters.PrimaryScreenWidth - wd - 50))
                                {
                                    dd[Convert.ToInt32(c.Margin.Top)] = false;
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
                            top = dd.Where(p => p.Value).Min(p => p.Key);
                        }
//                        v.Height = v.Text.DesiredSize.Height;
//                        v.Width = v.Text.DesiredSize.Width;
                        Storyboard s = new Storyboard();
                        Duration duration =
                            new Duration(
                                TimeSpan.FromTicks(Convert.ToInt64((SystemParameters.PrimaryScreenWidth + wd)/
                                                                   Store.FullOverlayEffect1*TimeSpan.TicksPerSecond)));
                        ThicknessAnimation f =
                            new ThicknessAnimation(new Thickness(SystemParameters.PrimaryScreenWidth, top, 0, 0),
                                new Thickness(-wd, top, 0, 0), duration);
                        s.Children.Add(f);
                        s.Duration = duration;
                        Storyboard.SetTarget(f, v);
                        Storyboard.SetTargetProperty(f, new PropertyPath("(FrameworkElement.Margin)"));
                        fulloverlay.LayoutRoot.Children.Add(v);
                        s.Completed += s_Completed;
                        s.Begin();
                    }
                }
            }
            else
            {
                log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AddDMText(user, text)));
            }
        }

        private void s_Completed(object sender, EventArgs e)
        {
            var s = sender as ClockGroup;
            if (s == null) return;
            var c = Storyboard.GetTarget(s.Children[0].Timeline) as FullScreenDanmaku;
            if (c != null)
            {
                fulloverlay.LayoutRoot.Children.Remove(c);
            }
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            var s = sender as ClockGroup;
            if (s == null) return;
            var c = Storyboard.GetTarget(s.Children[2].Timeline) as DanmakuTextControl;
            if (c != null)
            {
                overlay.LayoutRoot.Children.Remove(c);
            }
        }

        public void Test_OnClick(object sender, RoutedEventArgs e)
        {
//            logging("投喂记录不会在弹幕模式上出现, 这不是bug");
            Random ran = new Random();

            int n = ran.Next(100);
            if (n > 98)
            {
                AddDMText("彈幕姬報告", "這不是個測試", false);
            }
            else{
            AddDMText("彈幕姬報告", "這是一個測試", false);
            }
//            logging(DateTime.Now.Ticks+"");
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

      

        private void Disconnbtn_OnClick(object sender, RoutedEventArgs e)
        {
            b.Disconnect();
            this.connbtn.IsEnabled = true;
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void ClearMe_OnClick(object sender, RoutedEventArgs e)
        {
            SessionItems.Clear();
            
        }
    }

    public class SessionItem : INotifyPropertyChanged
    {
        private string _userName;
        private string _item;
        private decimal _num;

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == _userName) return;
                _userName = value;
                OnPropertyChanged();
            }
        }

        public string Item
        {
            get { return _item; }
            set
            {
                if (value == _item) return;
                _item = value;
                OnPropertyChanged();
            }
        }

        public decimal num
        {
            get { return _num; }
            set
            {
                if (value == _num) return;
                _num = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}