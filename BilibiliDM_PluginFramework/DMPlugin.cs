using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace BilibiliDM_PluginFramework
{

   
    public  class DMPlugin: DispatcherObject,INotifyPropertyChanged
    {
        private bool _status = false;
        public event ReceivedDanmakuEvt ReceivedDanmaku;
        public event DisconnectEvt Disconnected;
        public event ReceivedRoomCountEvt ReceivedRoomCount;
        public event ConnectedEvt Connected;

         public  void MainConnected(int roomid)
         {
             this.RoomID = roomid;
            try
            {
                Connected?.Invoke(null, new ConnectedEvtArgs() { roomid = roomid });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "插件" + PluginName + "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給該插件作者 " + PluginAuth + ", 聯繫方式 " + PluginCont);
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬插件" + PluginName + "錯誤報告.txt"))
                    {
                        outfile.WriteLine("請有空發給聯繫方式 " + PluginCont + " 謝謝");
                        outfile.Write(ex.ToString());
                    }

                }
                catch (Exception)
                {

                }
            }
            
         }

        public void MainReceivedDanMaku(ReceivedDanmakuArgs e)
        {
            try
            {
                ReceivedDanmaku?.Invoke(null, e);
            }
            catch (Exception ex)
            {

                MessageBox.Show(
                    "插件" + PluginName + "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給該插件作者 " + PluginAuth + ", 聯繫方式 " + PluginCont);
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬插件" + PluginName + "錯誤報告.txt"))
                    {
                        outfile.WriteLine("請有空發給聯繫方式 " + PluginCont + " 謝謝");
                        outfile.Write(ex.ToString());
                    }

                }
                catch (Exception)
                {

                }
            }
            
        }

        public void MainReceivedRoomCount(ReceivedRoomCountArgs e)
        {
            try
            {
                ReceivedRoomCount?.Invoke(null, e);
            }
            catch (Exception ex)
            {

                MessageBox.Show(
                    "插件" + PluginName + "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給該插件作者 " + PluginAuth + ", 聯繫方式 " + PluginCont);
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬插件" + PluginName + "錯誤報告.txt"))
                    {
                        outfile.WriteLine("請有空發給聯繫方式 " + PluginCont + " 謝謝");
                        outfile.Write(ex.ToString());
                    }

                }
                catch (Exception)
                {

                }
            }
           
        }

        public void MainDisconnected()
        {
            this.RoomID = null;
            try
            {
                Disconnected?.Invoke(null, null);
            }
            catch (Exception ex)
            {

                MessageBox.Show(
                    "插件" + PluginName + "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給該插件作者 " + PluginAuth + ", 聯繫方式 " + PluginCont);
                try
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                    using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬插件" + PluginName + "錯誤報告.txt"))
                    {
                        outfile.WriteLine("請有空發給聯繫方式 " + PluginCont + " 謝謝");
                        outfile.Write(ex.ToString());
                    }

                }
                catch (Exception)
                {

                }
            }
           
        }

        /// <summary>
        /// 插件名稱
        /// </summary>
        public string PluginName { get; set; } = "這是插件";

        /// <summary>
        /// 插件作者
        /// </summary>
        public string PluginAuth { get; set; } = "CopyLiu";

        /// <summary>
        /// 插件作者聯繫方式
        /// </summary>
        public string PluginCont { get; set; } = "copyliu@gmail.com";

        /// <summary>
        /// 插件版本號
        /// </summary>
        public string PluginVer { get; set; } = "0.0.1";

        /// <summary>
        /// 插件狀態
        /// </summary>
        public bool Status
        {
            get { return _status; }
            private set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        /// <summary>
        /// 當前連接中的房間
        /// </summary>
        public int? RoomId => RoomID;

        private int? RoomID;

        public DMPlugin()
        {
                
        }
        /// <summary>
        /// 啟用插件方法 請重寫此方法
        /// </summary>
        public virtual void Start()
        {
            this.Status = true;
            Console.WriteLine(this.PluginName+" Start!");
        }
        /// <summary>
        /// 禁用插件方法 請重寫此方法
        /// </summary>
        public virtual void Stop()
        {

            this.Status = false;
            Console.WriteLine(this.PluginName + " Stop!");
        }
        /// <summary>
        /// 管理插件方法 請重寫此方法
        /// </summary>
        public virtual void Admin()
        {
            
        }
        /// <summary>
        /// 反初始化方法, 在弹幕姬主程序退出时调用, 若有需要请重写,
        /// </summary>
        public virtual void DeInit()
        {
            
        }
        /// <summary>
        /// 打日志
        /// </summary>
        /// <param name="text"></param>
        public void Log(string text)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                dynamic mw = Application.Current.MainWindow;
                mw.logging(this.PluginName + " " + text);

            }));
            
        }
        /// <summary>
        /// 打彈幕
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fullscreen"></param>
        public void AddDM(string text, bool fullscreen = false)
        {

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                dynamic mw = Application.Current.MainWindow;
                mw.AddDMText(this.PluginName, text, true, fullscreen);

            }));
           
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

  
}
