using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Deployment.Application;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml.Serialization;
using BilibiliDM_PluginFramework;
using BiliDMLib;

namespace Bililive_dm
{
    /// <summary>
    ///     MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WsExTransparent = 0x20;
        private const int GwlExstyle = -20;
        private const int MaxCapacity = 100;

        private readonly Queue<DanmakuModel> _danmakuQueue = new Queue<DanmakuModel>();

        private readonly ObservableCollection<string> _messageQueue = new ObservableCollection<string>();
        private readonly DanmakuLoader b = new DanmakuLoader();
        private IDanmakuWindow _fulloverlay;
        public MainOverlay Overlay;
        private readonly ObservableCollection<DMPlugin> Plugins = new ObservableCollection<DMPlugin>();

        private readonly Thread ProcDanmakuThread;

        private readonly ObservableCollection<GiftRank> Ranking = new ObservableCollection<GiftRank>();
        private readonly ObservableCollection<SessionItem> SessionItems = new ObservableCollection<SessionItem>();

        private StoreModel _settings;

        private readonly StaticModel _static = new StaticModel();
        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer timer_magic;

        public MainWindow()
        {
            InitializeComponent();
            //reload becase window init will change some value
            Properties.Settings.Default.Reload();
            this._showvipEnabled = Properties.Settings.Default.showItem;
            this._overlayEnabled = Properties.Settings.Default.showSidebar;
            this._fulloverlayEnabled = Properties.Settings.Default.showFullWindow;
            this.Topmost = Properties.Settings.Default.onTop;

            try
            {
                this.RoomId.Text = Properties.Settings.Default.roomId.ToString();
            }
            catch
            {
                this.RoomId.Text = "";
            }
            
            var dt = new DateTime(2000, 1, 1);
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.FullName.Split(',')[1];

            var fullversion = version.Split('=')[1];
            var dates = int.Parse(fullversion.Split('.')[2]);

            var seconds = int.Parse(fullversion.Split('.')[3]);
            dt = dt.AddDays(dates);
            dt = dt.AddSeconds(seconds*2);
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Title += "   版本号: " +
                         ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            else
            {
                Title += "   *傻逼版本*";
            }
            Title += "   编译时间: " + dt;

            InitPlugins();
            Closed += MainWindow_Closed;
            HelpWeb.Source = new Uri("http://soft.ceve-market.org/bilibili_dm/app.htm?" + DateTime.Now.Ticks);
                //fuck you IE cache
            b.Disconnected += b_Disconnected;
            b.ReceivedDanmaku += b_ReceivedDanmaku;
            b.ReceivedRoomCount += b_ReceivedRoomCount;
            

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, FuckMicrosoft,
                Dispatcher);
            timer.Start();

            DataGrid2.ItemsSource = SessionItems;
//            fulloverlay.Show();

            Log.DataContext = _messageQueue;
//            log.ScrollToEnd();
            //            for (int i = 0; i < 150; i++)
            //            {
            //                logging("投喂记录不会在弹幕模式上出现, 这不是bug");
            //            }
            PluginGrid.ItemsSource = Plugins;

            if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1)
            {
                //MAGIC!
                timer_magic = new DispatcherTimer(new TimeSpan(0, 30, 0), DispatcherPriority.Normal, (sender, args) =>
                {
                    var query = Plugins.Where(p => p.PluginName.Contains("点歌"));
                    if (query.Any())
                    {
                        query.First().MainReceivedDanMaku(new ReceivedDanmakuArgs
                        {
                            Danmaku = new DanmakuModel
                            {
                                MsgType = MsgTypeEnum.Comment,
                                CommentText = "点歌 34376018",
                                CommentUser = "弹幕姬",
                                isAdmin = true,
                                isVIP = true
                            }
                        });
                    }
                }, Dispatcher);
                timer_magic.Start();
            }


            ProcDanmakuThread = new Thread(() =>
            {
                while (true)
                {
                    lock (_danmakuQueue)
                    {
                        var count = 0;
                        if (_danmakuQueue.Any())
                        {
                            count = (int) Math.Ceiling(_danmakuQueue.Count/30.0);
                        }

                        for (var i = 0; i < count; i++)
                        {
                            if (_danmakuQueue.Any())
                            {
                                var danmaku = _danmakuQueue.Dequeue();
                                ProcDanmaku(danmaku);
                                if (danmaku.MsgType == MsgTypeEnum.Comment)
                                {
                                    lock (_static)
                                    {
                                        _static.DanmakuCountShow += 1;
                                        _static.AddUser(danmaku.CommentUser);
                                    }
                                }
                            }
                        }
                    }

                    Thread.Sleep(30);
                }
            });
            ProcDanmakuThread.IsBackground = true;
            ProcDanmakuThread.Start();
            StaticPanel.DataContext = _static;


            for (var i = 0; i < 100; i++)
            {
                _messageQueue.Add("");
            }
            logging("投喂记录不会在弹幕模式上出现, 这不是bug");
            logging("可以点击日志复制到剪贴板");
            Loaded += MainWindow_Loaded;
        }

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Full.IsChecked = _fulloverlayEnabled;
            SideBar.IsChecked = _overlayEnabled;
            SaveLog.IsChecked = _savelogEnabled;
            SSTP.IsChecked = _sendsspEnabled;
            ShowItem.IsChecked = _showvipEnabled;
            ShowError.IsChecked = _showerrorEnabled;
            WindowTop.IsChecked = Topmost;
            var sc = Log.Template.FindName("LogScroll", Log) as ScrollViewer;
            sc?.ScrollToEnd();

            var shit = new Thread(() =>
            {
                var bbb = 5;
                while (true)
                {
                    var r = new Random();

                    lock (_danmakuQueue)
                    {
                        for (var i = 0; i < bbb; i++)
                        {
                            var a1 = r.NextDouble().ToString();
                            var b1 = r.NextDouble().ToString();
                            _danmakuQueue.Enqueue(new DanmakuModel

                            {
                                CommentUser = "asf",
                                CommentText = b1,
                                MsgType = MsgTypeEnum.Comment
                            });
                        }
                    }
                    lock (_static)
                    {
                        _static.DanmakuCountRaw += bbb;
                    }

                    Thread.Sleep(1000);
                }
            }
                );
            shit.IsBackground = true;

            //            shit.Start();



            try
            {
                var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                                                            IsolatedStorageScope.Domain |
                                                            IsolatedStorageScope.Assembly, null, null);
                var settingsreader =
                    new XmlSerializer(typeof(StoreModel));
                var reader = new StreamReader(new IsolatedStorageFileStream(
                    "settings.xml", FileMode.Open, isoStore));
                _settings = (StoreModel)settingsreader.Deserialize(reader);
                reader.Close();
            }
            catch (Exception)
            {
                _settings = new StoreModel();
            }
            _settings.SaveConfig();
            _settings.toStatic();
            OptionDialog.LayoutRoot.DataContext = _settings;
            // Full overlay need be loaded after _settings
            Full.IsChecked = _fulloverlayEnabled;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            foreach (var dmPlugin in Plugins)
            {
                try
                {
                    dmPlugin.DeInit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "插件" + dmPlugin.PluginName + "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給該插件作者 " + dmPlugin.PluginAuth + ", 聯繫方式 " +
                        dmPlugin.PluginCont);
                    try
                    {
                        var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                        using (var outfile = new StreamWriter(path + @"\B站彈幕姬插件" + dmPlugin.PluginName + "錯誤報告.txt"))
                        {
                            outfile.WriteLine("請有空發給聯繫方式 " + dmPlugin.PluginCont + " 謝謝");
                            outfile.Write(ex.ToString());
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        ~MainWindow()
        {
            if (_fulloverlay != null)
            {
                _fulloverlay.Dispose();
                _fulloverlay = null;
            }
        }

        private void FuckMicrosoft(object sender, EventArgs eventArgs)
        {
            if (_fulloverlay != null)
            {
                _fulloverlay.ForceTopmost();
            }
            if (Overlay != null)
            {
                Overlay.Topmost = false;
                Overlay.Topmost = true;
            }
        }

        private void OpenFullOverlay()
        {
            var win8Version = new Version(6, 2, 9200);
            var isWin8OrLater = Environment.OSVersion.Platform == PlatformID.Win32NT
                                && Environment.OSVersion.Version >= win8Version;
            if (isWin8OrLater && Store.WtfEngineEnabled)
                _fulloverlay = new WtfDanmakuWindow();
            else
                _fulloverlay = new WpfDanmakuOverlay();
            _settings.PropertyChanged += _fulloverlay.OnPropertyChanged;
            _fulloverlay.Show();
        }

        private void OpenOverlay()
        {
            Overlay = new MainOverlay();
            Overlay.Deactivated += overlay_Deactivated;
            Overlay.SourceInitialized += delegate
            {
                var hwnd = new WindowInteropHelper(Overlay).Handle;
                var extendedStyle = GetWindowLong(hwnd, GwlExstyle);
                SetWindowLong(hwnd, GwlExstyle, extendedStyle | WsExTransparent);
            };
            Overlay.Background = Brushes.Transparent;
            Overlay.ShowInTaskbar = false;
            Overlay.Topmost = true;
            Overlay.Top = SystemParameters.WorkArea.Top + Store.MainOverlayXoffset;
            Overlay.Left = SystemParameters.WorkArea.Right - Store.MainOverlayWidth + Store.MainOverlayYoffset;
            Overlay.Height = SystemParameters.WorkArea.Height;
            Overlay.Width = Store.MainOverlayWidth;
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
            int roomId;
            try
            {
                roomId = Convert.ToInt32(RoomId.Text.Trim());
            }
            catch (Exception)
            {
                MessageBox.Show("请输入房间号,房间号是!数!字!");
                return;
            }
            if (roomId > 0)
            {
                ConnBtn.IsEnabled = false;
                DisconnBtn.IsEnabled = false;
                var connectresult = false;
                logging("正在连接");
                connectresult = await b.ConnectAsync(roomId);
                while (!connectresult && sender == null && AutoReconnect.IsChecked == true)
                {
                    logging("正在连接");
                    connectresult = await b.ConnectAsync(roomId);
                }


                if (connectresult)
                {
                    errorlogging("連接成功");
                    AddDMText("彈幕姬報告", "連接成功", true);
                    SendSSP("連接成功");
                    Ranking.Clear();
                    SaveRoomId(roomId);

                    foreach (var dmPlugin in Plugins)
                    {
                        new Thread(() =>
                        {
                            try
                            {
                                dmPlugin.MainConnected(roomId);
                            }
                            catch (Exception ex)
                            {
                                Utils.PluginExceptionHandler(ex, dmPlugin);
                            }
                        }).Start();
                    }
                }
                else
                {
                    logging("連接失敗");
                    SendSSP("連接失敗");
                    AddDMText("彈幕姬報告", "連接失敗", true);

                    ConnBtn.IsEnabled = true;
                }
                DisconnBtn.IsEnabled = true;
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
            if (CheckAccess())
            {
                OnlineBlock.Text = e.UserCount + "";
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => { OnlineBlock.Text = e.UserCount + ""; }));
            }
            foreach (var dmPlugin in Plugins)
            {
                if (dmPlugin.Status)
                    new Thread(() =>
                    {
                        try
                        {
                            dmPlugin.MainReceivedRoomCount(e);
                        }
                        catch (Exception ex)
                        {
                            Utils.PluginExceptionHandler(ex, dmPlugin);
                        }
                    }).Start();
            }

            SendSSP("当前房间人数:" + e.UserCount);
        }

        private void b_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            foreach (var dmPlugin in Plugins)
            {
                if (dmPlugin.Status)
                    new Thread(() =>
                    {
                        try
                        {
                            dmPlugin.MainReceivedDanMaku(e);
                        }
                        catch (Exception ex)
                        {
                            Utils.PluginExceptionHandler(ex, dmPlugin);
                        }
                    }).Start();
            }

            if (e.Danmaku.MsgType == MsgTypeEnum.Comment)
            {
                lock (_static)
                {
                    _static.DanmakuCountRaw += 1;
                }
            }

            lock (_danmakuQueue)
            {
                var danmakuModel = e.Danmaku;
                _danmakuQueue.Enqueue(danmakuModel);
            }
        }

        private void ProcDanmaku(DanmakuModel danmakuModel)
        {
            switch (danmakuModel.MsgType)
            {
                case MsgTypeEnum.Comment:
                    logging("收到彈幕:" + (danmakuModel.isAdmin ? "[管]" : "") + (danmakuModel.isVIP ? "[爷]" : "") +
                            danmakuModel.CommentUser + " 說: " + danmakuModel.CommentText);

                    AddDMText(
                        (danmakuModel.isAdmin ? "[管]" : "") + (danmakuModel.isVIP ? "[爷]" : "") +
                        danmakuModel.CommentUser,
                        danmakuModel.CommentText);
                    SendSSP(string.Format(@"\_q{0}\n\_q\f[height,20]{1}",
                        (danmakuModel.isAdmin ? "[管]" : "") + (danmakuModel.isVIP ? "[爷]" : "") +
                        danmakuModel.CommentUser,
                        danmakuModel.CommentText));

                    break;
                case MsgTypeEnum.GiftTop:
                    foreach (var giftRank in danmakuModel.GiftRanking)
                    {
                        var query = Ranking.Where(p => p.uid == giftRank.uid);
                        if (query.Any())
                        {
                            var f = query.First();
                            Dispatcher.BeginInvoke(new Action(() => f.coin = giftRank.coin));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() => Ranking.Add(new GiftRank
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
                    lock (SessionItems)
                    {
                        var query =
                            SessionItems.Where(
                                p => p.UserName == danmakuModel.GiftUser && p.Item == danmakuModel.GiftName);
                        var sessionItems = query as SessionItem[] ?? query.ToArray();
                        if (sessionItems.Any())
                        {
                            Dispatcher.BeginInvoke(
                                new Action(() => sessionItems.First().num += Convert.ToDecimal(danmakuModel.GiftNum)));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() => SessionItems.Add(
                                new SessionItem
                                {
                                    Item = danmakuModel.GiftName,
                                    UserName = danmakuModel.GiftUser,
                                    num = Convert.ToDecimal(danmakuModel.GiftNum)
                                }
                                )));
                        }
                        logging("收到道具:" + danmakuModel.GiftUser + " 赠送的: " + danmakuModel.GiftName + " x " +
                                danmakuModel.GiftNum);
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (ShowItem.IsChecked == true)
                            {
                                AddDMText("收到道具",
                                    danmakuModel.GiftUser + " 赠送的: " + danmakuModel.GiftName + " x " +
                                    danmakuModel.GiftNum, true);
                            }
                        }));
                        break;
                    }
                }
                case MsgTypeEnum.Welcome:
                {
                    logging("欢迎老爷" + (danmakuModel.isAdmin ? "和管理" : "") + ": " + danmakuModel.CommentUser + " 进入直播间");
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ShowItem.IsChecked == true)
                        {
                            AddDMText("欢迎老爷" + (danmakuModel.isAdmin ? "和管理" : ""),
                                danmakuModel.CommentUser + " 进入直播间", true);
                        }
                    }));

                    break;
                }
            }
        }

        public void SendSSP(string msg)
        {
            if (SSTP.Dispatcher.CheckAccess())
            {
                if (SSTP.IsChecked == true)
                {
                    SSTPProtocol.SendSSPMsg(msg);
                }
            }
            else
            {
                SSTP.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => SendSSP(msg)));
            }
        }

        private void b_Disconnected(object sender, DisconnectEvtArgs args)
        {
            foreach (var dmPlugin in Plugins)
            {
                new Thread(() =>
                {
                    try
                    {
                        dmPlugin.MainDisconnected();
                    }
                    catch (Exception ex)
                    {
                        Utils.PluginExceptionHandler(ex, dmPlugin);
                    }
                }).Start();
            }

            errorlogging("連接被斷開: 开发者信息" + args.Error);
            AddDMText("彈幕姬報告", "連接被斷開", true);
            SendSSP("連接被斷開");
            if (CheckAccess())
            {
                if (AutoReconnect.IsChecked == true && args.Error != null)
                {
                    errorlogging("正在自动重连...");
                    AddDMText("彈幕姬報告", "正在自动重连", true);
                    connbtn_Click(null, null);
                }
                else
                {
                    ConnBtn.IsEnabled = true;
                }
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (AutoReconnect.IsChecked == true && args.Error != null)
                    {
                        errorlogging("正在自动重连...");
                        AddDMText("彈幕姬報告", "正在自动重连", true);
                        connbtn_Click(null, null);
                    }
                    else
                    {
                        ConnBtn.IsEnabled = true;
                    }
                }));
            }
        }

        private void errorlogging(string text)
        {
            if (!_showerrorEnabled) return;
            if (ShowError.Dispatcher.CheckAccess())
            {
                logging(text);
            }
            else
            {
                ShowError.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => errorlogging(text)));
            }
        }

        public void logging(string text)
        {
            if (Log.Dispatcher.CheckAccess())
            {
                if (_messageQueue.Count >= MaxCapacity)
                {
                    _messageQueue.RemoveAt(0);
                }

                _messageQueue.Add(DateTime.Now.ToString("T") + " : " + text);
//                this.log.Text = string.Join("\n", _messageQueue);
//                log.CaretIndex = this.log.Text.Length;


                if (_savelogEnabled)
                {
                    try
                    {
                        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


                        path = Path.Combine(path, "弹幕姬");
                        Directory.CreateDirectory(path);
                        using (
                            var outfile =
                                new StreamWriter(Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd") + ".txt"), true)
                            )
                        {
                            outfile.WriteLine(DateTime.Now.ToString("T") + " : " + text);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            else
            {
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => logging(text)));
            }
        }

        public void AddDMText(string user, string text, bool warn = false, bool foreceenablefullscreen = false)
        {
            if (!_showerrorEnabled && warn)
            {
                return;
            }
            if (!_overlayEnabled && !_fulloverlayEnabled) return;
            if (Dispatcher.CheckAccess())
            {
                if (SideBar.IsChecked == true)
                {
                    var c = new DanmakuTextControl();

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
                    Overlay.LayoutRoot.Children.Add(c);
                }
                if (Full.IsChecked == true && (!warn || foreceenablefullscreen))
                {
                    _fulloverlay.AddDanmaku(DanmakuType.Scrolling, text, 0xFFFFFFFF);
                }
            }
            else
            {
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => AddDMText(user, text)));
            }
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            var s = sender as ClockGroup;
            if (s == null) return;
            var c = Storyboard.GetTarget(s.Children[2].Timeline) as DanmakuTextControl;
            if (c != null)
            {
                Overlay.LayoutRoot.Children.Remove(c);
            }
        }

        public void Test_OnClick(object sender, RoutedEventArgs e)
        {
//            logging("投喂记录不会在弹幕模式上出现, 这不是bug");
            var ran = new Random();

            var n = ran.Next(100);
            if (n > 98)
            {
                AddDMText("彈幕姬報告", "這不是個測試", false);
            }
            else
            {
                AddDMText("彈幕姬報告", "這是一個測試", false);
            }
            SendSSP("彈幕姬測試");
            foreach (var dmPlugin in Plugins.Where(dmPlugin => dmPlugin.Status))
            {
                new Thread(() =>
                {
                    try
                    {
                        var m = new ReceivedDanmakuArgs
                        {
                            Danmaku =
                                new DanmakuModel
                                {
                                    CommentText = "插件彈幕測試",
                                    CommentUser = "彈幕姬",
                                    MsgType = MsgTypeEnum.Comment
                                }
                        };
                        dmPlugin.MainReceivedDanMaku(m);
                    }
                    catch (Exception ex)
                    {
                        Utils.PluginExceptionHandler(ex, dmPlugin);
                    }
                }).Start();
            }

//            logging(DateTime.Now.Ticks+"");
        }

        private void Full_Checked(object sender, RoutedEventArgs e)
        {
            //            overlay.Show();
            _fulloverlayEnabled = true;
            OpenFullOverlay();
            _fulloverlay.Show();
            Properties.Settings.Default.showFullWindow = true;
        }

        private void SideBar_Checked(object sender, RoutedEventArgs e)
        {
            _overlayEnabled = true;
            OpenOverlay();
            Overlay.Show();
            Properties.Settings.Default.showSidebar = true;
        }

        private void SideBar_Unchecked(object sender, RoutedEventArgs e)
        {
            _overlayEnabled = false;
            Overlay.Close();
            Properties.Settings.Default.showSidebar = false;
        }

        private void Full_Unchecked(object sender, RoutedEventArgs e)
        {
            _fulloverlayEnabled = false;
            _fulloverlay.Close();
            Properties.Settings.Default.showFullWindow = false;
        }


        private void Disconnbtn_OnClick(object sender, RoutedEventArgs e)
        {
            b.Disconnect();
            ConnBtn.IsEnabled = true;
            Properties.Settings.Default.Save();
            foreach (var dmPlugin in Plugins)
            {
                new Thread(() =>
                {
                    try
                    {
                        dmPlugin.MainDisconnected();
                    }
                    catch (Exception ex)
                    {
                        Utils.PluginExceptionHandler(ex, dmPlugin);
                    }
                }).Start();
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void ClearMe_OnClick(object sender, RoutedEventArgs e)
        {
            lock (SessionItems)
            {
                SessionItems.Clear();
            }
        }

        private void ClearMe2_OnClick(object sender, RoutedEventArgs e)
        {
            lock (_static)
            {
                _static.DanmakuCountShow = 0;
            }
        }

        private void ClearMe3_OnClick(object sender, RoutedEventArgs e)
        {
            lock (_static)
            {
                _static.ClearUser();
            }
        }

        private void ClearMe4_OnClick(object sender, RoutedEventArgs e)
        {
            lock (_static)
            {
                _static.DanmakuCountRaw = 0;
            }
        }

        private void Plugin_Enable(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;

            var contextMenu = (ContextMenu) menuItem.Parent;

            var item = (DataGrid) contextMenu.PlacementTarget;
            if (item.SelectedCells.Count == 0) return;
            var plugin = item.SelectedCells[0].Item as DMPlugin;
            if (plugin == null) return;

            try
            {
                if (!plugin.Status) plugin.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "插件" + plugin.PluginName + "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給該插件作者 " + plugin.PluginAuth + ", 聯繫方式 " +
                    plugin.PluginCont);
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    using (var outfile = new StreamWriter(path + @"\B站彈幕姬插件" + plugin.PluginName + "錯誤報告.txt"))
                    {
                        outfile.WriteLine("請有空發給聯繫方式 " + plugin.PluginCont + " 謝謝");
                        outfile.WriteLine(DateTime.Now + " " + plugin.PluginName + " " + plugin.PluginVer);
                        outfile.Write(ex.ToString());
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void Plugin_Disable(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;

            var contextMenu = (ContextMenu) menuItem.Parent;

            var item = (DataGrid) contextMenu.PlacementTarget;
            if (item.SelectedCells.Count == 0) return;
            var plugin = item.SelectedCells[0].Item as DMPlugin;
            if (plugin == null) return;

            try
            {
                if (plugin.Status) plugin.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "插件" + plugin.PluginName + "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給該插件作者 " + plugin.PluginAuth + ", 聯繫方式 " +
                    plugin.PluginCont);
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    using (var outfile = new StreamWriter(path + @"\B站彈幕姬插件" + plugin.PluginName + "錯誤報告.txt"))
                    {
                        outfile.WriteLine("請有空發給聯繫方式 " + plugin.PluginCont + " 謝謝");
                        outfile.WriteLine(DateTime.Now + " " + plugin.PluginName + " " + plugin.PluginVer);
                        outfile.Write(ex.ToString());
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void Plugin_admin(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem) sender;

            var contextMenu = (ContextMenu) menuItem.Parent;

            var item = (DataGrid) contextMenu.PlacementTarget;
            if (item.SelectedCells.Count == 0) return;
            var plugin = item.SelectedCells[0].Item as DMPlugin;
            if (plugin == null) return;

            try
            {
                plugin.Admin();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "插件" + plugin.PluginName + "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給該插件作者 " + plugin.PluginAuth + ", 聯繫方式 " +
                    plugin.PluginCont);
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    using (var outfile = new StreamWriter(path + @"\B站彈幕姬插件" + plugin.PluginName + "錯誤報告.txt"))
                    {
                        outfile.WriteLine(DateTime.Now + " " + "請有空發給聯繫方式 " + plugin.PluginCont + " 謝謝");
                        outfile.WriteLine(plugin.PluginName + " " + plugin.PluginVer);
                        outfile.Write(ex.ToString());
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void InitPlugins()
        {
            Plugins.Add(new MobileService());
            var path = "";
            try
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


                path = Path.Combine(path, "弹幕姬", "Plugins");
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                return;
            }
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                try
                {
                    var dll = Assembly.LoadFrom(file);
                    foreach (var exportedType in dll.GetExportedTypes())
                    {
                        if (exportedType.BaseType == typeof (DMPlugin))
                        {
                            var plugin = (DMPlugin) Activator.CreateInstance(exportedType);

                            Plugins.Add(plugin);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void WindowTop_OnChecked(object sender, RoutedEventArgs e)
        {
            Topmost = WindowTop.IsChecked == true;
            Properties.Settings.Default.onTop = Topmost;
        }

        private void SaveLog_OnChecked(object sender, RoutedEventArgs e)
        {
            _savelogEnabled = true;
        }

        private void SaveLog_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _savelogEnabled = false;
        }

        private void ShowItem_OnChecked(object sender, RoutedEventArgs e)
        {
            _showvipEnabled = true;
            Properties.Settings.Default.showItem = true;
        }

        private void ShowItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _showvipEnabled = false;
            Properties.Settings.Default.showItem = false;
        }

        private void SSTP_OnChecked(object sender, RoutedEventArgs e)
        {
            _sendsspEnabled = true;
        }

        private void SSTP_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _sendsspEnabled = false;
        }

        private void ShowError_OnChecked(object sender, RoutedEventArgs e)
        {
            _showerrorEnabled = true;
        }

        private void ShowError_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _showerrorEnabled = false;
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var textBlock = sender as TextBlock;
                if (textBlock != null)
                {
                    Clipboard.SetText(textBlock.Text);
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() => { MessageBox.Show("本行记录已复制到剪贴板"); }));
                }
            }
            catch (Exception)
            {
            }
        }

        private void SaveRoomId(int roomId)
        {
            try
            {
                Properties.Settings.Default.roomId = roomId;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                // ignored
            }
            //Do whatever you want here..
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (!this.ConnBtn.IsEnabled)
                {
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #region Runtime settings

        private bool _fulloverlayEnabled;
        private bool _overlayEnabled;
        private bool _savelogEnabled = true;
        private bool _sendsspEnabled = true;
        private bool _showvipEnabled;
        private bool _showerrorEnabled = true;

        #endregion

    }
}