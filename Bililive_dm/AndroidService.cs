using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BilibiliDM_PluginFramework;
using Bililive_dm.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bililive_dm
{
    public sealed class MobileService : DMPlugin
    {
        private static readonly int MAX_THREAD = 4;
        private readonly NamedPipeServerStream[] pipeServers = new NamedPipeServerStream[MAX_THREAD];
        private readonly Task[] Tasks = new Task[MAX_THREAD];

        public MobileService()
        {
            PluginDesc = "这是流氓插件, 用来和提词版联动";
            PluginAuth = "CopyLiu";
            PluginCont = "copyliu@gmail.com";
            PluginName = "提词版服务端";
            PluginVer = "⑨";
            ReceivedDanmaku += B_ReceivedDanmaku;
            Connected += OnConnected;
            Disconnected += OnDisconnected;
            ReceivedRoomCount += OnReceivedRoomCount;
            for (var i = 0; i < MAX_THREAD; i++) Tasks[i] = ServerTask(i);
        }

        private void OnReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
            if (!Status) return;
            foreach (var pipeServer in pipeServers)
                if (pipeServer?.IsConnected == true)
                {
                    var obj =
                        JObject.FromObject(new
                        {
                            User = "提示", Comment = $"當前氣人值:{e.UserCount}",
                            e.UserCount
                        });
                    SendMsg(pipeServer, obj);
                }
        }

        private void OnDisconnected(object sender, DisconnectEvtArgs e)
        {
            if (!Status) return;
            foreach (var pipeServer in pipeServers)
                if (pipeServer?.IsConnected == true)
                {
                    var obj =
                        JObject.FromObject(new
                            { User = "提示", Comment = "連接已斷開" });
                    SendMsg(pipeServer, obj);
                }
        }

        private void OnConnected(object sender, ConnectedEvtArgs e)
        {
            if (!Status) return;
            foreach (var pipeServer in pipeServers)
                if (pipeServer?.IsConnected == true)
                {
                    var obj =
                        JObject.FromObject(new
                            { User = "提示", Comment = $"房間號 {e.roomid} 連接成功" });
                    SendMsg(pipeServer, obj);
                }
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            MessageBox.Show("本插件不允许停用");
        }

        public override void Inited()
        {
            base.Inited();

            Start();
        }

        private async Task ServerTask(int i)
        {
            while (true)
            {
                var ps = new PipeSecurity();
                ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    PipeAccessRights.FullControl, AccessControlType.Allow));
                ps.AddAccessRule(new PipeAccessRule(
                    new SecurityIdentifier(
                        "S-1-15-2-4214749242-2175026965-4132357855-2536272452-2097044253-3453070321-328922716"),
                    PipeAccessRights.FullControl, AccessControlType.Allow));
                using (var pipeServer =
                       new NamedPipeServerStream(@"BiliLive_DM_PIPE", PipeDirection.Out, MAX_THREAD,
                           PipeTransmissionMode.Message, PipeOptions.None, 4096, 4096, ps))
                {
                    lock (pipeServers)
                    {
                        pipeServers[i] = pipeServer;
                    }

                    await pipeServer.WaitForConnectionAsync();
                    try
                    {
                        while (pipeServer.IsConnected) await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    catch (Exception e)
                    {
                        if (pipeServer.IsConnected) pipeServer.Close();
                    }
                }


                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }


        private void B_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            if (!Status) return;
            foreach (var pipeServer in pipeServers)
                if (pipeServer?.IsConnected == true)
                    switch (e.Danmaku.MsgType)
                    {
                        case MsgTypeEnum.Comment:
                        {
                            var obj =
                                JObject.FromObject(new
                                    { User = e.Danmaku.UserName + "", Comment = e.Danmaku.CommentText + "" });
                            SendMsg(pipeServer, obj);

                            break;
                        }
                        case MsgTypeEnum.GiftSend:
                        {
                            var cmt = string.Format(Resources.MainWindow_ProcDanmaku_收到道具__0__赠送的___1__x__2_,
                                e.Danmaku.UserName, e.Danmaku.GiftName, e.Danmaku.GiftCount);

                            var obj =
                                JObject.FromObject(new { User = "", Comment = cmt });
                            SendMsg(pipeServer, obj);

                            break;
                        }
                        case MsgTypeEnum.GuardBuy:
                        {
                            var cmt = string.Format(Resources.MainWindow_ProcDanmaku_上船__0__购买了__1__x__2_,
                                e.Danmaku.UserName, e.Danmaku.GiftName, e.Danmaku.GiftCount);
                            var obj =
                                JObject.FromObject(new { User = "", Comment = cmt });
                            SendMsg(pipeServer, obj);

                            break;
                        }
                        case MsgTypeEnum.SuperChat:
                        {
                            var obj =
                                JObject.FromObject(new
                                {
                                    User = e.Danmaku.UserName + " ￥:" + e.Danmaku.Price.ToString("N2"),
                                    Comment = e.Danmaku.CommentText + ""
                                });
                            SendMsg(pipeServer, obj);

                            break;
                        }
                        case MsgTypeEnum.Warning:
                        {
                            {
                                var obj =
                                    JObject.FromObject(
                                        new { User = "!!!!超管警告!!!!", Comment = e.Danmaku.CommentText + "" });
                                SendMsg(pipeServer, obj);

                                break;
                            }
                        }
                    }
        }

        private static void SendMsg(NamedPipeServerStream pipeServer, JObject obj)
        {
            var sendbuf = Encoding.UTF8.GetBytes(obj.ToString(Formatting.None) + "\r\n");
            lock (pipeServer)
            {
                try
                {
                    pipeServer.Write(sendbuf, 0, sendbuf.Length);
                }
                catch (IOException)
                {
                }
            }
        }


        // public override void Stop()
        // {
        //     MessageBox.Show("流氓插件不允许禁用", "报警了!");
        // }
    }
}