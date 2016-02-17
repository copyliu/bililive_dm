using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DMPluginTest
{
    public class Class1 : BilibiliDM_PluginFramework.DMPlugin
    {
        public Class1()
        {
            this.Connected += Class1_Connected;
            this.Disconnected += Class1_Disconnected;
            this.ReceivedDanmaku += Class1_ReceivedDanmaku;
            this.ReceivedRoomCount += Class1_ReceivedRoomCount;
            this.PluginAuth = "示例作者";
            this.PluginName = "示例插件";
            this.PluginCont = "example@example.com";
            this.PluginVer = "v0.0.1";
        }


        private void Class1_ReceivedRoomCount(object sender, BilibiliDM_PluginFramework.ReceivedRoomCountArgs e)
        {
           
        }

        private void Class1_ReceivedDanmaku(object sender, BilibiliDM_PluginFramework.ReceivedDanmakuArgs e)
        {
            throw new NotImplementedException();
            this.Log("BBB");
            this.AddDM("bbb",true);
        }

        private void Class1_Disconnected(object sender, BilibiliDM_PluginFramework.DisconnectEvtArgs e)
        {
            throw new NotImplementedException();
        }

        private void Class1_Connected(object sender, BilibiliDM_PluginFramework.ConnectedEvtArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Admin()
        {
            base.Admin();
            Console.WriteLine("Hello World");
            this.Log("BBB");
        }

        public override void Stop()
        {
            base.Stop();
            //請勿使用任何阻塞方法
            Console.WriteLine("Plugin Stoped!");
            this.Log("BBB");
        }

        public override void Start()
        {
            base.Start();
            //請勿使用任何阻塞方法
            Console.WriteLine("Plugin Started!");
            this.Log("BBB");
        }
    }
}
