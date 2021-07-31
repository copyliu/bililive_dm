using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BilibiliDM_PluginFramework;

namespace BililiveDebugPlugin
{
    
    public class DebugPlugin:BilibiliDM_PluginFramework.DMPlugin
    {
        private MainPage mp;
       
        public DebugPlugin()
        { 
            this.ReceivedDanmaku += OnReceivedDanmaku; 
            this.PluginAuth = "CopyLiu";
            this.PluginName = "開發員小工具";
            this.PluginCont = "copyliu@gmail.com";
            this.PluginVer = "v0.0.2";
            this.PluginDesc = "它看着很像F12";
        }

     

        private void OnReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            mp?.Dispatcher.BeginInvoke(DispatcherPriority.Normal,new Action(() =>
            {
                this.mp.context.DataList.Add(new DMItem()
                {
                    ItemName = DateTime.Now.ToString("hh:mm:ss")+" " + e.Danmaku.RawDataJToken["cmd"],
                    Model = e.Danmaku
                });
            }));
        }

     

        public override void Admin()
        {
            base.Admin();
            this.mp = new MainPage();
            this.mp.context.Plugin = this;
            this.mp.Closed += (sender, args) => this.mp = null;
            this.mp.Show();
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }
    }
}
