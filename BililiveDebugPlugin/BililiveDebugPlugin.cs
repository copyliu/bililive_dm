using System;
using System.Windows.Threading;
using BilibiliDM_PluginFramework;

namespace BililiveDebugPlugin
{
    public class DebugPlugin : DMPlugin
    {
        private MainPage mp;

        public DebugPlugin()
        {
            ReceivedDanmaku += OnReceivedDanmaku;
            PluginAuth = "CopyLiu";
            PluginName = "開發員小工具";
            PluginCont = "copyliu@gmail.com";
            PluginVer = "v0.0.2";
            PluginDesc = "它看着很像F12";
        }


        private void OnReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            mp?.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                mp.context.DataList.Add(new DMItem
                {
                    ItemName = DateTime.Now.ToString("HH:mm:ss") + " " + e.Danmaku.RawDataJToken["cmd"],
                    Model = e.Danmaku
                });
            }));
        }


        public override void Admin()
        {
            base.Admin();
            mp = new MainPage();
            mp.context.Plugin = this;
            mp.Closed += (sender, args) => mp = null;
            mp.Show();
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