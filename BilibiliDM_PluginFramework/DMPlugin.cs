using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace BilibiliDM_PluginFramework
{

   
    public  class DMPlugin:INotifyPropertyChanged
    {
        private bool _status = false;
        public event ReceivedDanmakuEvt ReceivedDanmaku;
        public event DisconnectEvt Disconnected;
        public event ReceivedRoomCountEvt ReceivedRoomCount;
        public event ConnectedEvt Connected;

         public  void MainConnected(int roomid)
         {
             Connected?.Invoke(null,new ConnectedEvtArgs() {roomid = roomid} );
         }

        public void MainReceivedDanMaku(ReceivedDanmakuArgs e)
        {
            ReceivedDanmaku?.Invoke(null,e);
        }

        public void MainReceivedRoomCount(ReceivedRoomCountArgs e)
        {
            ReceivedRoomCount?.Invoke(null,e);
        }

        public void MainDisconnected()
        {
            Disconnected?.Invoke(null,null);
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
                OnPropertyChanged();
            }
        }


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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

  
}
