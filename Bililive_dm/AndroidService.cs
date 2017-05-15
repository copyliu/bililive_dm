using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using BilibiliDM_PluginFramework;
using BiliDMLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bililive_dm
{
    public sealed class MobileService:DMPlugin
    {
        public MobileService()
        {
            this.PluginDesc = "这是流氓插件, 用来和题词版联动";
            this.PluginAuth = "CopyLiu";
            this.PluginCont = "copyliu@gmail.com";
            this.PluginName = "题词版服务端";
            this.PluginVer = "⑨";
            this.ReceivedDanmaku += B_ReceivedDanmaku;
//            this.Start();
        }



        private void B_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            try
            {
                if (e.Danmaku.MsgType == MsgTypeEnum.Comment)
                {
                    foreach (var i in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                        foreach (var ua in i.GetIPProperties().UnicastAddresses)
                        {
                        
                    UdpClient client = new UdpClient();
                    IPEndPoint ip = new IPEndPoint(ua.Address.GetBroadcastAddress(ua.IPv4Mask), 45695);
                    var obj =
                        JObject.FromObject(new {User = e.Danmaku.UserName + "", Comment = e.Danmaku.CommentText + ""});
                    byte[] sendbuf = Encoding.UTF8.GetBytes(obj.ToString());
                    client.Send(sendbuf, sendbuf.Length, ip);
                    client.Close();
                }

            }
            }
            catch (Exception)
            {
                
            }
            
        }

//        public override void Stop()
//        {
//            MessageBox.Show("流氓插件不允许禁用", "报警了!");
//        }
    }
}