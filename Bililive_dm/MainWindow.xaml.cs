using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Navigation;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using ContextMenu = System.Windows.Controls.ContextMenu;
using DataGrid = System.Windows.Controls.DataGrid;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;

namespace Bililive_dm
{
    /// <summary>
    ///     MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = -20;
        private const int _maxCapacity = 100;

        private readonly Queue<DanmakuModel> _danmakuQueue = new Queue<DanmakuModel>();

        private readonly ObservableCollection<string> _messageQueue = new ObservableCollection<string>();
        private readonly DanmakuLoader b = new DanmakuLoader();
        private IDanmakuWindow fulloverlay;
        public MainOverlay overlay;

        private readonly Thread ProcDanmakuThread;

        private readonly ObservableCollection<GiftRank> Ranking = new ObservableCollection<GiftRank>();
        private readonly ObservableCollection<SessionItem> SessionItems = new ObservableCollection<SessionItem>();

        private StoreModel settings;

        private readonly StaticModel Static = new StaticModel();
        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer timer_magic;
        private Thread releaseThread;
        private Regex FilterRegex;

        private bool net461 = false;

        private  void Get45or451FromRegistry()
        {
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                int releaseKey = Convert.ToInt32(ndpKey?.GetValue("Release"));
                if (releaseKey >= 394254)
                {
                    net461 = true;
                }
                else
                {
                    net461 = false;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Get45or451FromRegistry();
            if (!net461)
            {
                MessageBox.Show(this,
                    @"弹幕姬决定下版本开始停止支持Windows XP, 最低要求更改至 Win7 和 .NET 4.6.1, 
为保证你能在今后使用弹幕姬, 请安装最新的 .net framework .");
            }
            HelpWeb.Navigated+=HelpWebOnNavigated;
            //初始化日志


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
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            }
            catch (ConfigurationErrorsException ex)
            {//重置修复错误的配置文件
                string filename = ex.Filename;
                File.Delete(filename);
                Properties.Settings.Default.Reload();

            }

            try
            {
                this.RoomId.Text = Properties.Settings.Default.roomId.ToString();
            }
            catch
            {
                this.RoomId.Text = "";
            }

            var cmd_args = Environment.GetCommandLineArgs();
            debug_mode = cmd_args.Contains("-d") || cmd_args.Contains("--debug");
            rawoutput_mode = cmd_args.Contains("-r") || cmd_args.Contains("--raw");
            var offline_mode = cmd_args.Contains("-o") || cmd_args.Contains("--offline");

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
#if !DEBUG
                if(!(Debugger.IsAttached || offline_mode))
                {
                    MessageBox.Show(Application.Current.MainWindow, "你的打开方式不正确");
                    this.Close();
                }
#endif
            }
            if (debug_mode)
            {
                Title += "   *Debug模式*";
            }
            if (rawoutput_mode)
            {
                Title += "   *原始数据输出*";
            }
            Title += "   编译时间: " + dt;

            Closed += MainWindow_Closed;
            
            b.Disconnected += b_Disconnected;
            b.ReceivedDanmaku += b_ReceivedDanmaku;
            b.ReceivedRoomCount += b_ReceivedRoomCount;
            b.LogMessage += b_LogMessage;


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
            PluginGrid.ItemsSource = App.Plugins;

            if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1)
            {
                //MAGIC!
                timer_magic = new DispatcherTimer(new TimeSpan(0, 30, 0), DispatcherPriority.Normal, (sender, args) =>
                {
                    Magic();
                }, Dispatcher);
                timer_magic.Start();
            }

            releaseThread = new Thread(() =>
            {
                while (true)
                {
                    Utils.ReleaseMemory(true);
                    Thread.Sleep(30*1000);
                }
            });
            releaseThread.IsBackground = true;
            //            releaseThread.Start();
            ProcDanmakuThread = new Thread(() =>
            {
                while (true)
                {
                    lock (_danmakuQueue)
                    {
                        var count = 0;
                        if (_danmakuQueue.Any())
                        {
                            count = (int)Math.Ceiling(_danmakuQueue.Count / 30.0);
                        }

                        for (var i = 0; i < count; i++)
                        {
                            if (_danmakuQueue.Any())
                            {
                                var danmaku = _danmakuQueue.Dequeue();
                                if (danmaku.MsgType == MsgTypeEnum.Comment && enable_regex)
                                {
                                    if (FilterRegex.IsMatch(danmaku.CommentText)) continue;
                                 
                                }

                                if (danmaku.MsgType == MsgTypeEnum.Comment && ignorespam_enabled)
                                {
                                    try
                                    {
                                        var jobj = (JObject) danmaku.RawDataJToken;
                                        if (jobj["info"][0][9].Value<int>() != 0)
                                        {
                                            continue;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                      
                                    }
                                  
                                }
                                ProcDanmaku(danmaku);
                                if (danmaku.MsgType == MsgTypeEnum.Comment)
                                {
                                    lock (Static)
                                    {
                                        Static.DanmakuCountShow += 1;
                                        Static.AddUser(danmaku.UserName);
                                    }
                                }
                            }
                        }
                    }

                    Thread.Sleep(30);
                }
            })
            {
                IsBackground = true
            };
            ProcDanmakuThread.Start();
            StaticPanel.DataContext = Static;

            for (var i = 0; i < 100; i++)
            {
                _messageQueue.Add("");
            }
            if (!net461)
            {
                logging(
                    @"弹幕姬决定下版本开始停止支持Windows XP, 最低要求更改至 Win7 和 .NET 4.6.1, 
为保证你能在今后使用弹幕姬, 请安装最新的 .net framework .");
            }
            logging("公告1: 由于廉价的证书供应商被某虎公司搞死了于是没钱买证书了, 所以使用本软件时可能会跳安全提示请无需理会. ");
            logging("公告2: 最近出现了一些所谓弹幕姬后续版, 声称本软件已经不再更新, 并自称后续版是原作者开发是新版云云. ");
            logging("对此进行澄清, 弹幕姬从来也不会有任何的所谓后续版, 原作者直播间ID是5051, 任何非本人所开发的软件的一切问题均不能解答. ");
            logging("也请用户在咨询时, 确认清楚你所使用的软件类型, 向正确的关系者咨询. 谢谢. ");
            logging("公告3: 投喂记录不会在弹幕模式上出现, 这不是bug");
            logging("可以点击日志复制到剪贴板");
            if (debug_mode)
            {
                logging("当前为Debug模式");
            }

            InitPlugins();
            Loaded += MainWindow_Loaded;
        }

        private void HelpWebOnNavigated(object o, NavigationEventArgs navigationEventArgs)
        {
            HelpWeb.Navigated-=HelpWebOnNavigated;
            HelpWeb.Source = new Uri("https://soft.ceve-market.org/bilibili_dm/app.htm?" + DateTime.Now.Ticks);
            //fuck you IE cache

        }

        private void b_LogMessage(object sender, LogMessageArgs e)
        {
            logging(e.message);
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

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Full.IsChecked = fulloverlay_enabled;
            SideBar.IsChecked = overlay_enabled;
            SaveLog.IsChecked = savelog_enabled;
            SSTP.IsChecked = sendssp_enabled;
            EnableRegex.IsChecked = enable_regex;
            IgnoreSpam.IsChecked = ignorespam_enabled;
            ShowItem.IsChecked = showvip_enabled;
            ShowError.IsChecked = showerror_enabled;
            regex = Regex.Text.Trim();
            FilterRegex=new Regex(regex);
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
                                    UserName = "asf",
                                    CommentText = b1,
                                    MsgType = MsgTypeEnum.Comment
                                });
                            }
                        }
                        lock (Static)
                        {
                            Static.DanmakuCountRaw += bbb;
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
                settings = (StoreModel) settingsreader.Deserialize(reader);
                reader.Close();
            }
            catch (Exception)
            {
                settings = new StoreModel();
            }
            settings.SaveConfig();
            settings.toStatic();
            OptionDialog.LayoutRoot.DataContext = settings;
           
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            foreach (var dmPlugin in App.Plugins)
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
            if (fulloverlay != null)
            {
                fulloverlay.Dispose();
                fulloverlay = null;
            }
        }

        private void FuckMicrosoft(object sender, EventArgs eventArgs)
        {
            if (fulloverlay != null)
            {
                fulloverlay.ForceTopmost();
            }
            if (overlay != null)
            {
                overlay.Topmost = false;
                overlay.Topmost = true;
            }
        }

        private void OpenFullOverlay()
        {
            var win8Version = new Version(6, 2, 9200);
            var isWin8OrLater = Environment.OSVersion.Platform == PlatformID.Win32NT
                                && Environment.OSVersion.Version >= win8Version;
            if (isWin8OrLater && Store.WtfEngineEnabled)
                fulloverlay = new WtfDanmakuWindow();
            else
                fulloverlay = new WpfDanmakuOverlay();
            settings.PropertyChanged += fulloverlay.OnPropertyChanged;
            fulloverlay.Show();
        }

        private void OpenOverlay()
        {
            overlay = new MainOverlay();
            overlay.Deactivated += overlay_Deactivated;
            overlay.SourceInitialized += delegate
            {
                var hwnd = new WindowInteropHelper(overlay).Handle;
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
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
                var trytime = 0;
                logging("正在连接");

                if (debug_mode)
                {
                    logging("连接房间号：" + roomId);
                }

                connectresult = await b.ConnectAsync(roomId);

                if (!connectresult && b.Error != null)// 如果连接不成功并且出错了
                {
                    logging("出错信息：" + b.Error.ToString());
                }

                while (!connectresult && sender == null && AutoReconnect.IsChecked == true)
                {
                    if(trytime > 5)
                        break;
                    else
                        trytime++;

                    await Task.Delay(1000); // 稍等一下
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

                    foreach (var dmPlugin in App.Plugins)
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
                if (debug_mode)
                {
                    logging("直播间人数：" + e.UserCount);
                }
                OnlineBlock.Text = e.UserCount + "";
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => { OnlineBlock.Text = e.UserCount + ""; }));
            }
            foreach (var dmPlugin in App.Plugins)
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
            if (e.Danmaku.MsgType == MsgTypeEnum.Comment)
            {
                lock (Static)
                {
                    Static.DanmakuCountRaw += 1;
                }
            }

            lock (_danmakuQueue)
            {
                var danmakuModel = e.Danmaku;
                _danmakuQueue.Enqueue(danmakuModel);
            }

            foreach(var dmPlugin in App.Plugins)
            {
                if(dmPlugin.Status)
                    new Thread(() =>
                    {
                        try
                        {
                            dmPlugin.MainReceivedDanMaku(e);
                        }
                        catch(Exception ex)
                        {
                            Utils.PluginExceptionHandler(ex, dmPlugin);
                        }
                    }).Start();
            }
        }

        private void ProcDanmaku(DanmakuModel danmakuModel)
        {
            switch (danmakuModel.MsgType)
            {
                case MsgTypeEnum.Comment:
                    logging("收到彈幕:" + (danmakuModel.isAdmin ? "[管]" : "") + (danmakuModel.isVIP ? "[爷]" : "") +
                            danmakuModel.UserName + " 說: " + danmakuModel.CommentText);

                    AddDMText(
                        (danmakuModel.isAdmin ? "[管]" : "") + (danmakuModel.isVIP ? "[爷]" : "") +
                        danmakuModel.UserName,
                        danmakuModel.CommentText);
                    SendSSP(string.Format(@"\_q{0}\n\_q\f[height,20]{1}",
                        (danmakuModel.isAdmin ? "[管]" : "") + (danmakuModel.isVIP ? "[爷]" : "") +
                        danmakuModel.UserName,
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
                                p => p.UserName == danmakuModel.UserName && p.Item == danmakuModel.GiftName).ToArray();
                        if (query.Any())
                        {
                            Dispatcher.BeginInvoke(
                                new Action(() => query.First().num += Convert.ToDecimal(danmakuModel.GiftCount)));
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                lock (SessionItems)
                                {
                                    SessionItems.Add(
                                        new SessionItem
                                        {
                                            Item = danmakuModel.GiftName,
                                            UserName = danmakuModel.UserName,
                                            num = Convert.ToDecimal(danmakuModel.GiftCount)
                                        }
                                    );

                                }
                            }));
                        }
                        logging("收到道具:" + danmakuModel.UserName + " 赠送的: " + danmakuModel.GiftName + " x " +
                                danmakuModel.GiftCount);
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (ShowItem.IsChecked == true)
                            {
                                AddDMText("收到道具",
                                    danmakuModel.UserName + " 赠送的: " + danmakuModel.GiftName + " x " +
                                    danmakuModel.GiftCount, true);
                            }
                        }));
                        break;
                    }
                }
                case MsgTypeEnum.GuardBuy:
                {
                    logging("上船:" + danmakuModel.UserName + " 购买了 " + danmakuModel.GiftName + " x " + danmakuModel.GiftCount);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if(ShowItem.IsChecked == true)
                        {
                            AddDMText("上船",
                                danmakuModel.UserName + " 购买了 " + danmakuModel.GiftName + " x " +
                                danmakuModel.GiftCount, true);
                        }
                    }));
                    break;
                }
                case MsgTypeEnum.Welcome:
                {
                    logging("欢迎老爷" + (danmakuModel.isAdmin ? "和管理" : "") + ": " + danmakuModel.UserName + " 进入直播间");
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (ShowItem.IsChecked == true)
                        {
                            AddDMText("欢迎老爷" + (danmakuModel.isAdmin ? "和管理" : ""),
                                danmakuModel.UserName + " 进入直播间", true);
                        }
                    }));

                    break;
                }
                case MsgTypeEnum.WelcomeGuard:
                    {
                        string guard_text = string.Empty;
                        switch(danmakuModel.UserGuardLevel)
                        {
                            case 1:
                                guard_text = "总督";
                                break;
                            case 2:
                                guard_text = "提督";
                                break;
                            case 3:
                                guard_text = "舰长";
                                break;
                        }
                        logging("欢迎" + guard_text + ": " + danmakuModel.UserName + " 进入直播间");
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if(ShowItem.IsChecked == true)
                            {
                                AddDMText("欢迎" + guard_text, danmakuModel.UserName + " 进入直播间", true);
                            }
                        }));
                        break;
                    }
            }
            if (rawoutput_mode)
            {
                logging(danmakuModel.RawDataJToken.ToString());
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
            foreach (var dmPlugin in App.Plugins)
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
            if (!showerror_enabled) return;
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
                lock (_messageQueue)
                {
                    if (_messageQueue.Count >= _maxCapacity)
                    {
                        _messageQueue.RemoveAt(0);
                    }

                    _messageQueue.Add(DateTime.Now.ToString("T") + " : " + text);
                    //                this.log.Text = string.Join("\n", _messageQueue);
                    //                log.CaretIndex = this.log.Text.Length;

                }
                if (savelog_enabled)
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
            }
            else
            {
                Log.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => logging(text)));
            }
        }

        public void AddDMText(string user, string text, bool warn = false, bool foreceenablefullscreen = false)
        {
            if (!showerror_enabled && warn)
            {
                return;
            }
            if (!overlay_enabled && !fulloverlay_enabled) return;
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
                    overlay.LayoutRoot.Children.Add(c);
                }
                if (Full.IsChecked == true && (!warn || foreceenablefullscreen))
                {
                    fulloverlay.AddDanmaku(DanmakuType.Scrolling, text, 0xFFFFFFFF);
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
                overlay.LayoutRoot.Children.Remove(c);
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
            foreach (var dmPlugin in App.Plugins.Where(dmPlugin => dmPlugin.Status))
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
            }

//            logging(DateTime.Now.Ticks+"");
        }

        private void Full_Checked(object sender, RoutedEventArgs e)
        {
            //            overlay.Show();
            fulloverlay_enabled = true;
            OpenFullOverlay();
            fulloverlay.Show();
        }

        private void SideBar_Checked(object sender, RoutedEventArgs e)
        {
            overlay_enabled = true;
            OpenOverlay();
            overlay.Show();
        }

        private void SideBar_Unchecked(object sender, RoutedEventArgs e)
        {
            overlay_enabled = false;
            overlay.Close();
        }

        private void Full_Unchecked(object sender, RoutedEventArgs e)
        {
            fulloverlay_enabled = false;
            fulloverlay.Close();
        }


        private void Disconnbtn_OnClick(object sender, RoutedEventArgs e)
        {
            b.Disconnect();
            ConnBtn.IsEnabled = true;
            foreach (var dmPlugin in App.Plugins)
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
            lock (Static)
            {
                Static.DanmakuCountShow = 0;
            }
        }

        private void ClearMe3_OnClick(object sender, RoutedEventArgs e)
        {
            lock (Static)
            {
                Static.ClearUser();
            }
        }

        private void ClearMe4_OnClick(object sender, RoutedEventArgs e)
        {
            lock (Static)
            {
                Static.DanmakuCountRaw = 0;
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
            Stopwatch sw = new Stopwatch(); //new OverWatch(); （雾
            foreach (var file in files)
            {
                if (debug_mode)
                {
                    logging("加载插件文件：" + file);
                }
                try
                {
                    var dll = Assembly.LoadFrom(file);

                    if (debug_mode)
                    {
                        logging("Assembly.FullName == " + dll.FullName);
                        logging("Assembly.GetExportedTypes == " +
                                string.Join(",", dll.GetExportedTypes().Select(x => x.FullName).ToArray()));
                    }

                    foreach (var exportedType in dll.GetExportedTypes())
                    {
                        if (exportedType.BaseType == typeof(DMPlugin))
                        {
                            if (debug_mode)
                            {
                                sw.Restart();
                            }
                            var plugin = (DMPlugin) Activator.CreateInstance(exportedType);
                            if (debug_mode)
                            {
                                sw.Stop();
                                logging(
                                    $"插件{exportedType.FullName}({plugin.PluginName})加载完毕，用时{sw.ElapsedMilliseconds}ms");
                            }
                            App.Plugins.Add(plugin);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (debug_mode)
                    {
                        logging("加载出错：" + ex.ToString());
                    }
                }
            }

            foreach(var plugin in App.Plugins)
            {
                try
                {
                    plugin.Inited();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(
                        "插件" + plugin.PluginName + "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給該插件作者 " + plugin.PluginAuth + ", 聯繫方式 " +
                        plugin.PluginCont);
                    try
                    {
                        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                        using(var outfile = new StreamWriter(desktop + @"\B站彈幕姬插件" + plugin.PluginName + "錯誤報告.txt"))
                        {
                            outfile.WriteLine(DateTime.Now + " " + "請有空發給聯繫方式 " + plugin.PluginCont + " 謝謝");
                            outfile.WriteLine(plugin.PluginName + " " + plugin.PluginVer);
                            outfile.Write(ex.ToString());
                        }
                    }
                    catch(Exception)
                    {
                    }
                }

            }

        }

        private void OpenPluginFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "弹幕姬", "Plugins");
            if(Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(path);
                    Process.Start(path);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("打开插件文件夹出错\n信息"+ex.Message, "打开插件文件夹出错", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void WindowTop_OnChecked(object sender, RoutedEventArgs e)
        {
            Topmost = WindowTop.IsChecked == true;
        }

        private void SaveLog_OnChecked(object sender, RoutedEventArgs e)
        {
            savelog_enabled = true;
        }

        private void SaveLog_OnUnchecked(object sender, RoutedEventArgs e)
        {
            savelog_enabled = false;
        }

        private void ShowItem_OnChecked(object sender, RoutedEventArgs e)
        {
            showvip_enabled = true;
        }

        private void ShowItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            showvip_enabled = false;
        }

        private void SSTP_OnChecked(object sender, RoutedEventArgs e)
        {
            sendssp_enabled = true;
        }

        private void SSTP_OnUnchecked(object sender, RoutedEventArgs e)
        {
            sendssp_enabled = false;
        }

        private void ShowError_OnChecked(object sender, RoutedEventArgs e)
        {
            showerror_enabled = true;
        }

        private void ShowError_OnUnchecked(object sender, RoutedEventArgs e)
        {
            showerror_enabled = false;
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

        #region Runtime settings

        private bool fulloverlay_enabled;
        private bool overlay_enabled = true;
        private bool savelog_enabled = true;
        private bool sendssp_enabled = true;
        private bool showvip_enabled = true;
        private bool showerror_enabled = true;
        private bool rawoutput_mode = false;
        private bool enable_regex = false;
        private string regex = "";
        private bool ignorespam_enabled = false;

        public bool debug_mode { get; private set; }

        #endregion

        private void Magic_clicked(object sender, RoutedEventArgs e)
        {
            Magic();
        }
        private void Enableregex_OnChecked(object sender, RoutedEventArgs e)
        {
            enable_regex = true;
        }

        private void Enableregex_OnUnchecked(object sender, RoutedEventArgs e)
        {
            enable_regex = false;
        }

        private void Regex_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                FilterRegex = new Regex(Regex.Text.Trim());
            }
            catch (Exception exception)
            {
                
            }
        }

        private void IgnoreSpam_OnChecked(object sender, RoutedEventArgs e)
        {
            ignorespam_enabled = true;
            //TODO 保存配置
        }

        private void IgnoreSpam_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ignorespam_enabled = false;
            //TODO 保存配置
        }
    }
}