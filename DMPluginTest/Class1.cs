using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DMPluginTest
{
    public class Class1:BilibiliDM_PluginFramework.DMPlugin
    {
        public Class1()
        {
            this.Connected += Class1_Connected;
            this.Disconnected += Class1_Disconnected;
            this.ReceivedDanmaku += Class1_ReceivedDanmaku;
            this.ReceivedRoomCount += Class1_ReceivedRoomCount;

        }


        private void Class1_ReceivedRoomCount(object sender, BilibiliDM_PluginFramework.ReceivedRoomCountArgs e)
        {
           throw new NotImplementedException();
        }

        private void Class1_ReceivedDanmaku(object sender, BilibiliDM_PluginFramework.ReceivedDanmakuArgs e)
        {
            throw new NotImplementedException();
        }

        private void Class1_Disconnected(object sender, BilibiliDM_PluginFramework.DisconnectEvtArgs e)
        {
            throw new NotImplementedException();
        }

        private void Class1_Connected(object sender, BilibiliDM_PluginFramework.ConnectedEvtArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
