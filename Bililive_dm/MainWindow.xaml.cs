using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml.Serialization;
using BilibiliDM_PluginFramework;
using BiliDMLib;
using Bililive_dm.Annotations;
using Bililive_dm.Properties;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace Bililive_dm
{
    using static WINAPI.USER32;

    /// <summary>
    ///     MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : StyledWindow
    {
        /// <summary>
        ///     主日誌窗口最大消息數
        /// </summary>
        private const int MAX_CAPACITY = 100;

        /// <summary>
        ///     彈幕連接實例
        /// </summary>
        private readonly DanmakuLoader _b = new DanmakuLoader();

        /// <summary>
        ///     消息隊列
        /// </summary>
        private readonly Queue<DanmakuModel> _danmakuQueue = new Queue<DanmakuModel>();

        /// <summary>
        ///     主日誌窗口條目
        /// </summary>
        private readonly ObservableCollection<string> _messageQueue = new ObservableCollection<string>();

        private readonly ObservableCollection<GiftRank> _ranking = new ObservableCollection<GiftRank>();
        private readonly ObservableCollection<SessionItem> _sessionItems = new ObservableCollection<SessionItem>();

        private readonly StoreModel _settings;

        private readonly StaticModel _static = new StaticModel();
        private Regex _filterRegex;

        private IDanmakuWindow _fulloverlay;

        private bool _net461;
        public MainOverlay Overlay;

        public MainWindow()
        {
            InitializeComponent();
            switch (CultureInfo.DefaultThreadCurrentUICulture.TwoLetterISOLanguageName)
            {
                case "ja":
                    Width = 950;
                    Height = 550;
                    break;
            }

            Merged = Resources.MergedDictionaries;
            Merged.Add(new ResourceDictionary());

            Get45Or451FromRegistry();
            if (!_net461)
                MessageBox.Show(this,
                    Properties.Resources.MainWindow_MainWindow_NetError);
            HelpWeb.Navigated += HelpWebOnNavigated;
            //初始化日志
            // if (!(Debugger.IsAttached ))
            // {
            //     this.IsEnabled = false;
            // }

            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = Path.Combine(path, "弹幕姬");
                Directory.CreateDirectory(path);
                File.Create(Path.Combine(path, "lastrun.txt")).Close();
            }
            catch (Exception e)
            {
            }


            try
            {
                RoomId.Text = Settings.Default.roomId.ToString();
            }
            catch
            {
                RoomId.Text = "";
            }

            var cmdArgs = Environment.GetCommandLineArgs();
            DebugMode = cmdArgs.Contains("-d") || cmdArgs.Contains("--debug");
            rawoutput_mode = cmdArgs.Contains("-r") || cmdArgs.Contains("--raw");
            var offlineMode = cmdArgs.Contains("-o") || cmdArgs.Contains("--offline");

            var dt = new DateTime(2000, 1, 1);
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.FullName.Split(',')[1];

            var fullversion = version.Split('=')[1];
            var dates = int.Parse(fullversion.Split('.')[2]);

            var seconds = int.Parse(fullversion.Split('.')[3]);
            dt = dt.AddDays(dates);
            dt = dt.AddSeconds(seconds * 2);
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Title += Properties.Resources.MainWindow_MainWindow____版本号__ +
                         ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            else
            {
                Title += Properties.Resources.MainWindow_MainWindow_____傻逼版本_;
#if !DEBUG
                if(!(Debugger.IsAttached || offlineMode))
                {
                    MessageBox.Show(Application.Current.MainWindow, Properties.Resources.MainWindow_MainWindow_你的打开方式不正确);
                    this.Close();
                }
#endif
            }

            if (DebugMode) Title += Properties.Resources.MainWindow_MainWindow_____Debug模式_;
            if (rawoutput_mode) Title += Properties.Resources.MainWindow_MainWindow_____原始数据输出_;
            Title += Properties.Resources.MainWindow_MainWindow____编译时间__ + dt;

            Closed += MainWindow_Closed;

            _b.Disconnected += b_Disconnected;
            _b.ReceivedDanmaku += b_ReceivedDanmaku;
            _b.ReceivedRoomCount += b_ReceivedRoomCount;
            _b.LogMessage += b_LogMessage;


            var timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, FuckMicrosoft,
                Dispatcher);
            timer.Start();

            DataGrid2.ItemsSource = _sessionItems;
            //            fulloverlay.Show();

            Log.DataContext = _messageQueue;
            //            log.ScrollToEnd();
            //            for (int i = 0; i < 150; i++)
            //            {
            //                logging("投喂记录不会在弹幕模式上出现, 这不是bug");
            //            }
            PluginGrid.ItemsSource = App.Plugins;

            if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1)
            {
                //MAGIC!
                var timerMagic = new DispatcherTimer(new TimeSpan(0, 30, 0), DispatcherPriority.Normal,
                    (sender, args) => { Magic(); }, Dispatcher);
                timerMagic.Start();
            }
#if false
//釋放內存 但不需要
            var releaseThread = new Thread(() =>
            {
                while (true)
                {
                    Utils.ReleaseMemory(true);
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
            });
            releaseThread.IsBackground = true;
            //            releaseThread.Start();
#endif
            var procDanmakuThread = new Thread(() =>
            {
                while (true)
                {
                    lock (_danmakuQueue)
                    {
                        var count = 0;
                        if (_danmakuQueue.Any()) count = (int)Math.Ceiling(_danmakuQueue.Count / 30.0);

                        for (var i = 0; i < count; i++)
                        {
                            if (!_danmakuQueue.Any()) continue;
                            var danmaku = _danmakuQueue.Dequeue();
                            if (danmaku.MsgType == MsgTypeEnum.Comment && _enableRegex)
                                if (_filterRegex.IsMatch(danmaku.CommentText))
                                    continue;

                            if (danmaku.MsgType == MsgTypeEnum.Comment && _ignorespamEnabled)
                                try
                                {
                                    var jobj = (JObject)danmaku.RawDataJToken;
                                    if (jobj["info"][0][9].Value<int>() != 0) continue;
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                            if (danmaku.MsgType == MsgTypeEnum.Comment && _ignoreemojiEnabled)
                                try
                                {
                                    var jobj = (JObject)danmaku.RawDataJToken;
                                    if (jobj["info"][0][12].Value<int>() != 0) continue;
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                            ProcDanmaku(danmaku);
                            if (danmaku.MsgType != MsgTypeEnum.Comment) continue;
                            lock (_static)
                            {
                                _static.DanmakuCountShow += 1;
                                _static.AddUser(danmaku.UserName);
                            }
                        }
                    }

                    Thread.Sleep(30);
                }
            })
            {
                IsBackground = true
            };
            procDanmakuThread.Start();
            StaticPanel.DataContext = _static;

            for (var i = 0; i < 100; i++) _messageQueue.Add("");
            if (!_net461)
                Logging(
                    Properties.Resources.MainWindow_MainWindow_NetError);
            Logging(Properties.Resources.MainWindow_MainWindow_公告1);
            Logging(Properties.Resources.MainWindow_MainWindow_公告2);
            Logging(Properties.Resources.MainWindow_MainWindow_公告2_2);
            Logging(Properties.Resources.MainWindow_MainWindow_公告2_3);
            Logging(Properties.Resources.MainWindow_MainWindow_公告3);
            Logging(Properties.Resources.MainWindow_MainWindow_可以点击日志复制到剪贴板);
            if (DebugMode) Logging(Properties.Resources.MainWindow_MainWindow_当前为Debug模式);

            InitPlugins();

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

            Loaded += MainWindow_Loaded;
            // Log.Loaded += (sender, args) => { LogScroll.ScrollToEnd(); };
            Log.Loaded += (sender, args) =>
            {
                var sc = Log.Template.FindName("LogScroll", Log) as ScrollViewer;
                sc?.ScrollToEnd();
            };

        }

        private Collection<ResourceDictionary> Merged { get; }

        private void Get45Or451FromRegistry()
        {
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                       .OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
            {
                var releaseKey = Convert.ToInt32(ndpKey?.GetValue("Release"));
                if (releaseKey >= 394254)
                    _net461 = true;
                else
                    _net461 = false;
            }
        }


        private void HelpWebOnNavigated(object o, NavigationEventArgs navigationEventArgs)
        {
            HelpWeb.Navigated -= HelpWebOnNavigated;
            HelpWeb.Source = new Uri("https://soft.ceve-market.org/bilibili_dm/app.htm?" + DateTime.Now.Ticks);
            //fuck you IE cache
            HelpWeb.ObjectForScripting = new ObjectForScriptingHelper(this);
        }

        private void b_LogMessage(object sender, LogMessageArgs e)
        {
            Logging(e.message);
        }

        private void Magic()
        {
            //            var query = Plugins.Where(p => p.PluginName.Contains("点歌"));
            //            if (query.Any())
            //            {
            //                if (!query.First().Status) return;
            //                var ran = new Random();
            //
            //                var n = ran.Next(2);
            //                if (n == 1)
            //                {
            //                    try
            //                    {
            //                        query.First().MainReceivedDanMaku(new ReceivedDanmakuArgs
            //                        {
            //                            Danmaku = new DanmakuModel
            //                            {
            //                                MsgType = MsgTypeEnum.Comment,
            //                                CommentText = "强点 34376018",
            //                                UserName = "弹幕姬",
            //                                isAdmin = true,
            //                                isVIP = true
            //                            }
            //                        });
            //                    }
            //                    catch (Exception)
            //                    {
            //
            //                    }
            //
            //                }
            //                else
            //                {
            //                    try
            //                    {
            //                        var plugin = query.First();
            //                        var T = plugin.GetType();
            //                        var method = T.GetMethod("AddToPlayList");
            //                        method.Invoke(plugin,
            //                            new[] {"弹幕姬敬赠", "弹幕姬", "弹幕姬", "http://soft.ceve-market.org/bilibili_dm/1.mp3"});
            //                    }
            //                    catch (Exception)
            //                    {
            //
            //
            //                    }
            //
            //                }
            //            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Full.IsChecked = _fulloverlayEnabled;
            SideBar.IsChecked = _overlayEnabled;
            SaveLog.IsChecked = _savelogEnabled;
            SSTP.IsChecked = _sendsspEnabled;
            EnableRegex.IsChecked = _enableRegex;
            IgnoreEmoji.IsChecked = _ignoreemojiEnabled;
            IgnoreSpam.IsChecked = _ignorespamEnabled;
            ShowItem.IsChecked = _showvipEnabled;
            ShowInteract.IsChecked = _showInteractEnabled;
            ShowError.IsChecked = _showerrorEnabled;
            _regex = Regex.Text.Trim();
            _filterRegex = new Regex(_regex);

#if DEBUG
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
                                    UserName = "asf",
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
            )
            {
                IsBackground = true
            };

            //            shit.Start();
#endif

            OptionDialog.LayoutRoot.DataContext = _settings;
            DisplayAffinity.DataContext = _settings;
            _settings.PropertyChanged += (o, args) => { SetWindowAffinity(); };
            SetWindowAffinity();
        }

        private void SetWindowAffinity()
        {
            var wndHelper = new WindowInteropHelper(this);
            SetWindowDisplayAffinity(wndHelper.Handle,
                Store.DisplayAffinity ? WindowDisplayAffinity.ExcludeFromCapture : 0);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            foreach (var dmPlugin in App.Plugins)
                try
                {
                    dmPlugin.DeInit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(Properties.Resources.MainWindow_MainWindow_Closed_插件錯誤, dmPlugin.PluginName,
                            dmPlugin.PluginAuth, dmPlugin.PluginCont));
                    try
                    {
                        var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                        using (var outfile = new StreamWriter(path + @"\B站彈幕姬插件" + dmPlugin.PluginName + "錯誤報告.txt"))
                        {
                            outfile.WriteLine(Properties.Resources.MainWindow_MainWindow_Closed_报错,
                                dmPlugin.PluginCont);
                            outfile.Write(ex.ToString());
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
        }

        ~MainWindow()
        {
            if (_fulloverlay == null) return;
            _fulloverlay.Dispose();
            _fulloverlay = null;
        }

        private void FuckMicrosoft(object sender, EventArgs eventArgs)
        {
            _fulloverlay?.ForceTopmost();
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
                var hWnd = new WindowInteropHelper(Overlay).Handle;
                var exStyles = GetExtendedWindowStyles(hWnd);
                SetExtendedWindowStyles(hWnd, exStyles | ExtendedWindowStyles.Transparent);
            };
            Overlay.Background = Brushes.Transparent;
            Overlay.ShowInTaskbar = false;
            Overlay.Topmost = true;
            Overlay.Top = SystemParameters.WorkArea.Top + Store.MainOverlayXoffset;
            Overlay.Left = SystemParameters.WorkArea.Right - Store.MainOverlayWidth + Store.MainOverlayYoffset;
            Overlay.Height = SystemParameters.WorkArea.Height;
            Overlay.Width = Store.MainOverlayWidth;
            _settings.PropertyChanged += Overlay.OnPropertyChanged;
        }

        private void overlay_Deactivated(object sender, EventArgs e)
        {
            if (sender is MainOverlay overlay) overlay.Topmost = true;
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
                MessageBox.Show(Properties.Resources.MainWindow_connbtn_Click_请输入房间号_房间号是_数_字_);
                return;
            }

            if (roomId > 0)
            {
                ConnBtn.IsEnabled = false;
                DisconnBtn.IsEnabled = false;
                var connectresult = false;
                var trytime = 0;
                Logging(Properties.Resources.MainWindow_connbtn_Click_正在连接);

                if (DebugMode) Logging(string.Format(Properties.Resources.MainWindow_connbtn_Click_, roomId));

                connectresult = await _b.ConnectAsync(roomId);

                if (!connectresult && _b.Error != null) // 如果连接不成功并且出错了
                    Logging(string.Format(Properties.Resources.MainWindow_connbtn_Click_出錯, _b.Error));

                while (!connectresult && sender == null && AutoReconnect.IsChecked == true)
                {
                    if (trytime > 5)
                        break;
                    trytime++;

                    await Task.Delay(1000); // 稍等一下
                    Logging(Properties.Resources.MainWindow_connbtn_Click_正在连接);
                    connectresult = await _b.ConnectAsync(roomId);
                }


                if (connectresult)
                {
                    Errorlogging(Properties.Resources.MainWindow_connbtn_Click_連接成功);
                    AddDMText(Properties.Resources.MainWindow_connbtn_Click_彈幕姬本身,
                        Properties.Resources.MainWindow_connbtn_Click_連接成功, true);
                    SendSSP(Properties.Resources.MainWindow_connbtn_Click_連接成功);
                    _ranking.Clear();
                    SaveRoomId(roomId);

                    foreach (var dmPlugin in App.Plugins)
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
                else
                {
                    Logging(Properties.Resources.MainWindow_connbtn_Click_連接失敗);
                    SendSSP(Properties.Resources.MainWindow_connbtn_Click_連接失敗);
                    AddDMText(Properties.Resources.MainWindow_connbtn_Click_彈幕姬本身,
                        Properties.Resources.MainWindow_connbtn_Click_連接失敗, true);

                    ConnBtn.IsEnabled = true;
                }

                DisconnBtn.IsEnabled = true;
            }
            else
            {
                MessageBox.Show(Properties.Resources.MainWindow_connbtn_Click_ID非法);
            }
        }

        private void b_ReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
            //            logging("當前房間人數:" + e.UserCount);
            //            AddDMText("當前房間人數", e.UserCount+"", true);
            //AddDMText(e.Danmaku.CommentUser, e.Danmaku.CommentText);
            if (CheckAccess())
            {
                if (DebugMode)
                    Logging(string.Format(Properties.Resources.MainWindow_b_ReceivedRoomCount_, e.UserCount));
                OnlineBlock.Text = e.UserCount + "";
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => { OnlineBlock.Text = e.UserCount + ""; }));
            }

            foreach (var dmPlugin in App.Plugins)
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

            SendSSP(string.Format(Properties.Resources.MainWindow_b_ReceivedRoomCount_, e.UserCount));
        }

        private void b_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            if (e.Danmaku.MsgType == MsgTypeEnum.Comment)
                lock (_static)
                {
                    _static.DanmakuCountRaw += 1;
                }


            lock (_danmakuQueue)
            {
                var danmakuModel = e.Danmaku;
                _danmakuQueue.Enqueue(danmakuModel);
            }

            foreach (var dmPlugin in App.Plugins)
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

        private void ProcDanmaku(DanmakuModel danmakuModel)
        {
            switch (danmakuModel.MsgType)
            {
                case MsgTypeEnum.Comment:
                    Logging(
                        string.Format(Properties.Resources.MainWindow_ProcDanmaku_收到彈幕__0__1__2__說___3_,
                            danmakuModel.isAdmin ? Properties.Resources.MainWindow_ProcDanmaku__管理員前綴_ : "",
                            danmakuModel.isVIP ? Properties.Resources.MainWindow_ProcDanmaku__VIP前綴 : "",
                            danmakuModel.UserName, danmakuModel.CommentText));

                    AddDMText(
                        (danmakuModel.isAdmin ? Properties.Resources.MainWindow_ProcDanmaku__管理員前綴_ : "") +
                        (danmakuModel.isVIP ? Properties.Resources.MainWindow_ProcDanmaku__VIP前綴 : "") +
                        danmakuModel.UserName,
                        danmakuModel.CommentText);
                    SendSSP(string.Format(Properties.Resources.MainWindow_ProcDanmaku___SSPCommentFormat,
                        (danmakuModel.isAdmin ? Properties.Resources.MainWindow_ProcDanmaku__管理員前綴_ : "") +
                        (danmakuModel.isVIP ? Properties.Resources.MainWindow_ProcDanmaku__VIP前綴 : "") +
                        danmakuModel.UserName,
                        danmakuModel.CommentText));

                    break;
                case MsgTypeEnum.SuperChat:
                {
                    Logging(
                        string.Format(Properties.Resources.SuperChatLogName,
                            danmakuModel.isAdmin ? Properties.Resources.MainWindow_ProcDanmaku__管理員前綴_ : "",
                            danmakuModel.isVIP ? Properties.Resources.MainWindow_ProcDanmaku__VIP前綴 : "",
                            danmakuModel.UserName, danmakuModel.CommentText));

                    AddDMText(
                        Properties.Resources.MainWindow_ProcDanmaku____SuperChat___ +
                        (danmakuModel.isAdmin ? Properties.Resources.MainWindow_ProcDanmaku__管理員前綴_ : "") +
                        (danmakuModel.isVIP ? Properties.Resources.MainWindow_ProcDanmaku__VIP前綴 : "") +
                        danmakuModel.UserName + " ￥:" + danmakuModel.Price.ToString("N2"),
                        danmakuModel.CommentText, keeptime: danmakuModel.SCKeepTime, warn: true);
                    SendSSP(string.Format(Properties.Resources.MainWindow_ProcDanmaku___SSPCommentFormat,
                        (danmakuModel.isAdmin ? Properties.Resources.MainWindow_ProcDanmaku__管理員前綴_ : "") +
                        (danmakuModel.isVIP ? Properties.Resources.MainWindow_ProcDanmaku__VIP前綴 : "") +
                        danmakuModel.UserName,
                        danmakuModel.CommentText));

                    break;
                }

                case MsgTypeEnum.GiftTop:
                    foreach (var giftRank in danmakuModel.GiftRanking)
                    {
                        var query = _ranking.Where(p => p.uid == giftRank.uid);
                        if (query.Any())
                        {
                            var f = query.First();
                            Dispatcher.BeginInvoke(new Action(() => f.coin = giftRank.coin));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() => _ranking.Add(new GiftRank
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
                    lock (_sessionItems)
                    {
                        var query =
                            _sessionItems.Where(
                                p => p.UserName == danmakuModel.UserName && p.Item == danmakuModel.GiftName).ToArray();
                        if (query.Any())
                            Dispatcher.BeginInvoke(
                                new Action(() => query.First().num += Convert.ToDecimal(danmakuModel.GiftCount)));
                        else
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                lock (_sessionItems)
                                {
                                    _sessionItems.Add(
                                        new SessionItem
                                        {
                                            Item = danmakuModel.GiftName,
                                            UserName = danmakuModel.UserName,
                                            num = Convert.ToDecimal(danmakuModel.GiftCount)
                                        }
                                    );
                                }
                            }));

                        Logging(string.Format(Properties.Resources.MainWindow_ProcDanmaku_收到道具__0__赠送的___1__x__2_,
                            danmakuModel.UserName, danmakuModel.GiftName, danmakuModel.GiftCount));
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (ShowItem.IsChecked == true)
                                AddDMText(Properties.Resources.MainWindow_ProcDanmaku_收到道具,
                                    string.Format(Properties.Resources.MainWindow_ProcDanmaku__0__赠送的___1__x__2_,
                                        danmakuModel.UserName, danmakuModel.GiftName, danmakuModel.GiftCount), true);
                        }));
                        break;
                    }
                }
                case MsgTypeEnum.GuardBuy:
                {
                    Logging(string.Format(Properties.Resources.MainWindow_ProcDanmaku_上船__0__购买了__1__x__2_,
                        danmakuModel.UserName, danmakuModel.GiftName, danmakuModel.GiftCount));
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ShowItem.IsChecked == true)
                            AddDMText(string.Format(Properties.Resources.MainWindow_ProcDanmaku_上船__0__购买了__1__x__2_,
                                danmakuModel.UserName, danmakuModel.GiftName, danmakuModel.GiftCount), null, true);
                    }));
                    break;
                }
                case MsgTypeEnum.Welcome:
                {
                    string format;
                    if (danmakuModel.isAdmin)
                        format = Properties.Resources.MainWindow_ProcDanmaku_歡迎老爺__0__進入直播間;
                    else
                        format = Properties.Resources.MainWindow_ProcDanmaku_歡迎老爺和管理員__0__進入直播間;

                    var text = string.Format(format, danmakuModel.UserName);
                    Logging(text);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ShowItem.IsChecked == true) AddDMText(text, null, true);
                    }));

                    break;
                }
                case MsgTypeEnum.WelcomeGuard:
                {
                    var guard_text = string.Empty;
                    switch (danmakuModel.UserGuardLevel)
                    {
                        case 1:
                            guard_text = Properties.Resources.MainWindow_ProcDanmaku_总督;
                            break;
                        case 2:
                            guard_text = Properties.Resources.MainWindow_ProcDanmaku_提督;
                            break;
                        case 3:
                            guard_text = Properties.Resources.MainWindow_ProcDanmaku_舰长;
                            break;
                    }

                    Logging(
                        string.Format(Properties.Resources.MainWindow_ProcDanmaku_欢迎_0____1__2_, guard_text,
                            danmakuModel.UserName));
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ShowItem.IsChecked == true)
                            AddDMText(string.Format(Properties.Resources.MainWindow_ProcDanmaku_欢迎_0____1__2_,
                                guard_text,
                                danmakuModel.UserName), null, true);
                    }));
                    break;
                }
                case MsgTypeEnum.Interact:
                {
                    string text;
                    switch (danmakuModel.InteractType)
                    {
                        case InteractTypeEnum.Enter:
                            text = Properties.Resources.InteractType_Text1;
                            break;
                        case InteractTypeEnum.Follow:
                            text = Properties.Resources.InteractType_Text2;
                            break;
                        case InteractTypeEnum.Share:
                            text = Properties.Resources.InteractType_Text3;
                            break;
                        case InteractTypeEnum.SpecialFollow:
                            text = Properties.Resources.InteractType_Text4;
                            break;
                        case InteractTypeEnum.MutualFollow:
                            text = Properties.Resources.InteractType_Text5;
                            break;
                        default:
                            text = Properties.Resources.InteractType_Unknown;
                            break;
                    }

                    var logtext = string.Format(text, danmakuModel.UserName);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ShowInteract.IsChecked == true)
                        {
                            Logging(logtext);
                            AddDMText(logtext, null, true);
                        }
                    }));
                    break;
                }
                case MsgTypeEnum.Warning:
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Logging(Properties.Resources.MainWindow_ProcDanmaku_超管警告 + ":" + danmakuModel.CommentText);

                        AddDMText(Properties.Resources.MainWindow_ProcDanmaku_超管警告, danmakuModel.CommentText, false,
                            false, null, true);
                    }));
                    break;
                }
                case MsgTypeEnum.WatchedChange:
                {
                    Dispatcher.BeginInvoke(new Action(() => { WatchedBlock.Text = danmakuModel.WatchedCount + ""; }));
                    break;
                }
            }

            if (rawoutput_mode) Logging(danmakuModel.RawDataJToken.ToString());
        }

        // ReSharper disable once MemberCanBePrivate.Global
        [PublicAPI]
        public void SendSSP(string msg)
        {
            if (SSTP.Dispatcher.CheckAccess())
            {
                if (SSTP.IsChecked == true) SSTPProtocol.SendSSPMsg(msg);
            }
            else
            {
                SSTP.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => SendSSP(msg)));
            }
        }

        private async void b_Disconnected(object sender, DisconnectEvtArgs args)
        {
            foreach (var dmPlugin in App.Plugins)
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

            Errorlogging(string.Format(Properties.Resources.MainWindow_b_Disconnected_連接被斷開__开发者信息_0_, args.Error));
            AddDMText(Properties.Resources.MainWindow_connbtn_Click_彈幕姬本身,
                Properties.Resources.MainWindow_b_Disconnected_連接被斷開, true);
            SendSSP(Properties.Resources.MainWindow_b_Disconnected_連接被斷開);
            if (CheckAccess())
            {
                if (AutoReconnect.IsChecked == true && args.Error != null)
                {
                    Errorlogging(Properties.Resources.MainWindow_b_Disconnected_正在自动重连___);
                    AddDMText(Properties.Resources.MainWindow_connbtn_Click_彈幕姬本身,
                        Properties.Resources.MainWindow_b_Disconnected_正在自动重连___, true);
                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                    connbtn_Click(null, null);
                }
                else
                {
                    ConnBtn.IsEnabled = true;
                }
            }
            else
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (AutoReconnect.IsChecked == true && args.Error != null)
                    {
                        Errorlogging(Properties.Resources.MainWindow_b_Disconnected_正在自动重连___);
                        AddDMText(Properties.Resources.MainWindow_connbtn_Click_彈幕姬本身,
                            Properties.Resources.MainWindow_b_Disconnected_正在自动重连___, true);
                        connbtn_Click(null, null);
                    }
                    else
                    {
                        ConnBtn.IsEnabled = true;
                    }
                }));
            }
        }

        private void Errorlogging(string text)
        {
            if (!_showerrorEnabled) return;
            if (ShowError.Dispatcher.CheckAccess())
                Logging(text);
            else
                ShowError.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Errorlogging(text)));
        }

// ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006 // 命名样式
        [PublicAPI]
        public void logging(string text)
#pragma warning restore IDE1006 // 命名样式
        {
            this.Logging(text);
            
        }
        private void Logging(string text)
        {
            if (Log.Dispatcher.CheckAccess())
            {
                lock (_messageQueue)
                {
                    if (_messageQueue.Count >= MAX_CAPACITY) _messageQueue.RemoveAt(0);

                    _messageQueue.Add(DateTime.Now.ToString("T") + " : " + text);
                    //                this.log.Text = string.Join("\n", _messageQueue);
                    //                log.CaretIndex = this.log.Text.Length;

                    // sc?.ScrollToEnd();
                }

                if (!_savelogEnabled) return;
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

                    using (
                        var outfile =
                        new StreamWriter(Path.Combine(path, "lastrun.txt"), true)
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
            else
            {
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Logging(text)));
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        [PublicAPI]
        public void AddDMText(string user, string text, bool warn = false, bool foreceenablefullscreen = false,
            int? keeptime = null, bool red = false)
        {
            if (!_showerrorEnabled && warn) return;
            if (!_overlayEnabled && !_fulloverlayEnabled) return;
            if (Dispatcher.CheckAccess())
            {
                if (SideBar.IsChecked == true)
                {
                    var c = new DanmakuTextControl(keeptime ?? 0, red);

                    c.UserName.Text = user;
                    if (warn) c.UserName.Foreground = Brushes.Red;

                    if (string.IsNullOrEmpty(text))
                    {
                        c.Text.Text = "";
                        c.sp.Text = "";
                    }
                    else
                    {
                        c.Text.Text = text;
                    }

                    c.ChangeHeight();
                    var sb = (Storyboard)c.Resources["Storyboard1"];
                    //Storyboard.SetTarget(sb,c);
                    sb.Completed += sb_Completed;
                    Overlay.LayoutRoot.Children.Add(c);
                }

                if (Full.IsChecked == true && (!warn || foreceenablefullscreen))
                    _fulloverlay.AddDanmaku(DanmakuType.Scrolling, text, 0xFFFFFFFF);
            }
            else
            {
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() => AddDMText(user, text, warn, foreceenablefullscreen, keeptime)));
            }
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            var s = sender as ClockGroup;
            if (s == null) return;
            if (Storyboard.GetTarget(s.Children[2].Timeline) is DanmakuTextControl c)
                Overlay.LayoutRoot.Children.Remove(c);
        }

        public void Test_OnClick(object sender, RoutedEventArgs e)
        {
            //            logging("投喂记录不会在弹幕模式上出现, 这不是bug");
            var ran = new Random();
            // _danmakuQueue.Enqueue(new DanmakuModel("{\"cmd\":\"SUPER_CHAT_MESSAGE\",\"data\":{\"id\":\"200541\",\"uid\":18923374,\"price\":30,\"rate\":1000,\"message\":\"\\u4e09\\u4e03\\u662f\\u4e00\\u79cd\\u4e2d\\u836f\\u54e6\\uff08\\u836f\\u5b66\\u5b9d\\u8d1d\\u7684\\u80af\\u5b9a\\uff09\",\"trans_mark\":0,\"is_ranked\":0,\"message_trans\":\"\",\"background_image\":\"http:\\/\\/i0.hdslb.com\\/bfs\\/live\\/1aee2d5e9e8f03eed462a7b4bbfd0a7128bbc8b1.png\",\"background_color\":\"#EDF5FF\",\"background_icon\":\"\",\"background_price_color\":\"#7497CD\",\"background_bottom_color\":\"#2A60B2\",\"ts\":1586521245,\"token\":\"1018B059\",\"medal_info\":{\"icon_id\":0,\"target_id\":168598,\"special\":\"\",\"anchor_uname\":\"\\u900d\\u9065\\u6563\\u4eba\",\"anchor_roomid\":1017,\"medal_level\":11,\"medal_name\":\"\\u523a\\u513f\",\"medal_color\":\"#a068f1\"},\"user_info\":{\"uname\":\"\\u7ebf\\u7c92\\u4f53hl-s\",\"face\":\"http:\\/\\/i2.hdslb.com\\/bfs\\/face\\/c521ea6ef23c738b39f0823a18a7c0bcc1aedfa5.jpg\",\"face_frame\":\"http:\\/\\/i0.hdslb.com\\/bfs\\/live\\/78e8a800e97403f1137c0c1b5029648c390be390.png\",\"guard_level\":3,\"user_level\":10,\"level_color\":\"#969696\",\"is_vip\":0,\"is_svip\":0,\"is_main_vip\":1,\"title\":\"0\",\"manager\":0},\"time\":60,\"start_time\":1586521245,\"end_time\":1586521305,\"gift\":{\"num\":1,\"gift_id\":12000,\"gift_name\":\"\\u9192\\u76ee\\u7559\\u8a00\"}}}\r\n",2));

            var n = ran.Next(100);
            AddDMText(Properties.Resources.MainWindow_connbtn_Click_彈幕姬本身,
                n > 98
                    ? Properties.Resources.MainWindow_Test_OnClick_這不是個測試
                    : Properties.Resources.MainWindow_Test_OnClick_這是一個測試);
            SendSSP(Properties.Resources.MainWindow_Test_OnClick_彈幕姬測試);
            foreach (var dmPlugin in App.Plugins.Where(dmPlugin => dmPlugin.Status))
                new Thread(() =>
                {
                    try
                    {
                        var m = new ReceivedDanmakuArgs
                        {
                            Danmaku =
                                new DanmakuModel
                                {
                                    CommentText = Properties.Resources.MainWindow_Test_OnClick_插件彈幕測試,
                                    UserName = "彈幕姬",
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

            //            logging(DateTime.Now.Ticks+"");
        }

        private void Full_Checked(object sender, RoutedEventArgs e)
        {
            //            overlay.Show();
            _fulloverlayEnabled = true;
            OpenFullOverlay();
            //_fulloverlay.Show();
        }

        private void SideBar_Checked(object sender, RoutedEventArgs e)
        {
            _overlayEnabled = true;
            OpenOverlay();
            Overlay.Show();
        }

        private void SideBar_Unchecked(object sender, RoutedEventArgs e)
        {
            _overlayEnabled = false;
            Overlay.Close();
        }

        private void Full_Unchecked(object sender, RoutedEventArgs e)
        {
            _fulloverlayEnabled = false;
            _fulloverlay.Close();
        }


        private void Disconnbtn_OnClick(object sender, RoutedEventArgs e)
        {
            _b.Disconnect();
            ConnBtn.IsEnabled = true;
            foreach (var dmPlugin in App.Plugins)
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

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void ClearMe_OnClick(object sender, RoutedEventArgs e)
        {
            lock (_sessionItems)
            {
                _sessionItems.Clear();
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
            var menuItem = (MenuItem)sender;

            var contextMenu = (ContextMenu)menuItem.Parent;

            var item = (DataGrid)contextMenu.PlacementTarget;
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
                    string.Format(Properties.Resources.MainWindow_Plugin_Enable_插件報錯, plugin.PluginName,
                        plugin.PluginAuth, plugin.PluginCont));
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    using (var outfile = new StreamWriter(path + @"\B站彈幕姬插件" + plugin.PluginName + "錯誤報告.txt"))
                    {
                        outfile.WriteLine(Properties.Resources.MainWindow_Plugin_Enable_請有空發給聯繫方式__0__謝謝,
                            plugin.PluginCont);
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
            var menuItem = (MenuItem)sender;

            var contextMenu = (ContextMenu)menuItem.Parent;

            var item = (DataGrid)contextMenu.PlacementTarget;
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
                    string.Format(Properties.Resources.MainWindow_Plugin_Disable_插件報錯2, plugin.PluginName,
                        plugin.PluginAuth, plugin.PluginCont));
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    using (var outfile = new StreamWriter(path + @"\B站彈幕姬插件" + plugin.PluginName + "錯誤報告.txt"))
                    {
                        outfile.WriteLine(Properties.Resources.MainWindow_Plugin_Enable_請有空發給聯繫方式__0__謝謝,
                            plugin.PluginCont);
                        outfile.WriteLine(DateTime.Now + " " + plugin.PluginName + " " + plugin.PluginVer);
                        outfile.Write(ex.ToString());
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void Plugin_admin(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            var contextMenu = (ContextMenu)menuItem.Parent;

            var item = (DataGrid)contextMenu.PlacementTarget;
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
                    string.Format(Properties.Resources.MainWindow_Plugin_Disable_插件報錯2, plugin.PluginName,
                        plugin.PluginAuth, plugin.PluginCont));
                try
                {
                    var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    using (var outfile = new StreamWriter(path + @"\B站彈幕姬插件" + plugin.PluginName + "錯誤報告.txt"))
                    {
                        outfile.WriteLine(DateTime.Now + " " +
                                          string.Format(Properties.Resources.MainWindow_Plugin_Enable_請有空發給聯繫方式__0__謝謝,
                                              plugin.PluginCont));
                        outfile.WriteLine(plugin.PluginName + " " + plugin.PluginVer);
                        outfile.Write(ex.ToString());
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void InitPlugins()
        {
            App.Plugins.Add(new MobileService());
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
            var sw = new Stopwatch(); //new OverWatch(); （雾
            foreach (var file in files)
            {
                if (DebugMode) Logging("加载插件文件：" + file);
                try
                {
                    var dll = Assembly.LoadFrom(file);

                    if (DebugMode)
                    {
                        Logging("Assembly.FullName == " + dll.FullName);
                        Logging("Assembly.GetExportedTypes == " +
                                string.Join(",", dll.GetExportedTypes().Select(x => x.FullName).ToArray()));
                    }

                    foreach (var exportedType in dll.GetExportedTypes())
                    {
                        if (exportedType.BaseType != typeof(DMPlugin)) continue;
                        if (DebugMode) sw.Restart();
                        var plugin = (DMPlugin)Activator.CreateInstance(exportedType);
                        if (DebugMode)
                        {
                            sw.Stop();
                            Logging(
                                $"插件{exportedType.FullName}({plugin.PluginName})加载完毕，用时{sw.ElapsedMilliseconds}ms");
                        }

                        App.Plugins.Add(plugin);
                    }
                }
                catch (Exception ex)
                {
                    if (DebugMode) Logging("加载出错：" + ex);
                }
            }

            foreach (var plugin in App.Plugins)
                try
                {
                    plugin.Inited();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format(Properties.Resources.MainWindow_Plugin_Disable_插件報錯2, plugin.PluginName,
                            plugin.PluginAuth, plugin.PluginCont));
                    try
                    {
                        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                        using (var outfile = new StreamWriter(desktop + @"\B站彈幕姬插件" + plugin.PluginName + "錯誤報告.txt"))
                        {
                            outfile.WriteLine(DateTime.Now + " " + string.Format(
                                Properties.Resources.MainWindow_Plugin_Enable_請有空發給聯繫方式__0__謝謝, plugin.PluginCont));
                            outfile.WriteLine(plugin.PluginName + " " + plugin.PluginVer);
                            outfile.Write(ex.ToString());
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
        }

        private void OpenPluginFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "弹幕姬", "Plugins");
            if (Directory.Exists(path))
                Process.Start(path);
            else
                try
                {
                    Directory.CreateDirectory(path);
                    Process.Start(path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Properties.Resources.MainWindow_OpenPluginFolder_OnClick_ + ex.Message,
                        Properties.Resources.MainWindow_OpenPluginFolder_OnClick_打开插件文件夹出错, MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
        }

        private void WindowTop_OnChecked(object sender, RoutedEventArgs e)
        {
            Topmost = WindowTop.IsChecked == true;
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
        }

        private void ShowItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _showvipEnabled = false;
        }

        private void ShowInteract_OnChecked(object sender, RoutedEventArgs e)
        {
            _showInteractEnabled = true;
        }

        private void ShowInteract_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _showInteractEnabled = false;
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
                if (!(sender is TextBlock textBlock)) return;
                Clipboard.SetText(textBlock.Text);
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        MessageBox.Show(Properties.Resources.MainWindow_UIElement_OnMouseLeftButtonUp_本行记录已复制到剪贴板);
                    }));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void SaveRoomId(int roomId)
        {
            try
            {
                Settings.Default.roomId = roomId;
                Settings.Default.Save();
            }
            catch (Exception)
            {
                // ignored
            }
            //Do whatever you want here..
        }

        private void Magic_clicked(object sender, RoutedEventArgs e)
        {
            Magic();
        }

        private void Enableregex_OnChecked(object sender, RoutedEventArgs e)
        {
            _enableRegex = true;
        }

        private void Enableregex_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _enableRegex = false;
        }

        private void Regex_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _filterRegex = new Regex(Regex.Text.Trim());
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void IgnoreEmoji_OnChecked(object sender, RoutedEventArgs e)
        {
            _ignoreemojiEnabled = true;
        }

        private void IgnoreEmoji_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _ignoreemojiEnabled = false;
        }

        private void IgnoreSpam_OnChecked(object sender, RoutedEventArgs e)
        {
            _ignorespamEnabled = true;
            //TODO 保存配置
        }

        private void IgnoreSpam_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _ignorespamEnabled = false;
            //TODO 保存配置
        }

        private void SelectLanguage(object sender, RoutedEventArgs e)
        {
            var lg = new LanguageSelector();
            lg.Owner = this;
            lg.ShowDialog();
        }

        private void Skin_Click(object sender, RoutedEventArgs e)
        {
            var selector = new Selector
            {
                Owner = this,
                WindowStyle = WindowStyle.ToolWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var curr = App.Current.merged[0];
            var themes = selector.Themes;
            var candidates = themes.Where(item => item.Value == curr);
            var selected = candidates.SingleOrDefault();
            selector.list.SelectedItem = selected;

            selector.PreviewTheme += skin =>
            {
                if (skin == null) return;
                Merged[0] = skin;
            };

            if (selector.Select() is ResourceDictionary result) App.Current.merged[0] = result;
            Merged[0] = new ResourceDictionary();
        }

        #region Runtime settings

        private bool _fulloverlayEnabled;
        private bool _overlayEnabled = true;
        private bool _savelogEnabled = true;
        private bool _sendsspEnabled = true;
        private bool _showvipEnabled = true;
        private bool _showInteractEnabled = true;
        private bool _showerrorEnabled = true;
        private readonly bool rawoutput_mode;
        private bool _enableRegex;
        private string _regex = "";
        private bool _ignorespamEnabled;
        private bool _ignoreemojiEnabled;

        private bool DebugMode { get; }
        
        // ReSharper disable once InconsistentNaming
        [PublicAPI]
        public bool debug_mode => DebugMode;

        #endregion
    }
}
