using System;
using BilibiliDM_PluginFramework;

namespace DMPluginTest
{
    public class Class1 : DMPlugin
    {
        public Class1()
        {
            Connected += Class1_Connected;
            Disconnected += Class1_Disconnected;
            ReceivedDanmaku += Class1_ReceivedDanmaku;
            ReceivedRoomCount += Class1_ReceivedRoomCount;
            PluginAuth = "示例作者";
            PluginName = "示例插件";
            PluginCont = "example@example.com";
            PluginVer = "v0.0.1";
        }


        private void Class1_ReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
        }

        private void Class1_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            throw new NotImplementedException();
            Log("BBB");
            AddDM("bbb", true);
        }

        private void Class1_Disconnected(object sender, DisconnectEvtArgs e)
        {
            throw new NotImplementedException();
        }

        private void Class1_Connected(object sender, ConnectedEvtArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Admin()
        {
            base.Admin();
            Console.WriteLine("Hello World");
            Log("BBB");
        }

        public override void Stop()
        {
            base.Stop();
            //請勿使用任何阻塞方法
            Console.WriteLine("Plugin Stoped!");
            Log("BBB");
        }

        public override void Start()
        {
            base.Start();
            //請勿使用任何阻塞方法
            Console.WriteLine("Plugin Started!");
            Log("BBB");
        }
    }
}