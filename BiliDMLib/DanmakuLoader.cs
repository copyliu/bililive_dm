using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using BilibiliDM_PluginFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliDMLib
{
    public class DanmakuLoader
    {
        private string[] defaulthosts = new string[] {"livecmt-2.bilibili.com", "livecmt-1.bilibili.com"};
        private string ChatHost = "chat.bilibili.com";
        // private int ChatPort = 788;
        private int ChatPort = 2243; // TCP协议默认端口疑似修改到 2243
        private TcpClient Client;
        private NetworkStream NetStream;
//        private string RoomInfoUrl = "http://live.bilibili.com/sch_list/";
        private string CIDInfoUrl = "http://live.bilibili.com/api/player?id=cid:";
        private bool Connected = false;
        public Exception Error;
        public event ReceivedDanmakuEvt ReceivedDanmaku;
        public event DisconnectEvt Disconnected;
        public event ReceivedRoomCountEvt ReceivedRoomCount;
        public event LogMessageEvt LogMessage;
        private bool debuglog = true;
        private short protocolversion = 1;
        private static int lastroomid ;
        private static string lastserver;
//        private object shit_lock=new object();//ReceiveMessageLoop 似乎好像大概會同時運行兩個的bug, 但是不修了, 鎖上算了

        public async Task<bool> ConnectAsync(int roomId)
        {
            try
            {
                if (this.Connected) throw new InvalidOperationException();
                int channelId = roomId;
//
//                var request = WebRequest.Create(RoomInfoUrl + roomId + ".json");
//                var response = request.GetResponse();
//
//                int channelId;
//                using (var stream = response.GetResponseStream())
//                using (var sr = new StreamReader(stream))
//                {
//                    var json = await sr.ReadToEndAsync();
//                    Debug.WriteLine(json);
//                    dynamic jo = JObject.Parse(json);
//                    channelId = (int) jo.list[0].cid;
//                }

                if (channelId != lastroomid)
                {
                    try
                    {
                        var request2 = WebRequest.Create(CIDInfoUrl + channelId);
                        request2.Timeout = 2000;
                        var response2 = await request2.GetResponseAsync();
                        using (var stream = response2.GetResponseStream())
                        {
                            using (var sr = new StreamReader(stream))
                            {
                                var text = await sr.ReadToEndAsync();
                                var xml = "<root>" + text + "</root>";
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(xml);
                                ChatHost = doc["root"]["dm_server"].InnerText;
                                /*
                                 * 注：目前xml里存在 server 和 dm_server ，分别是不同的域名
                                 * 两个域名指向的服务器应该是相同的，测试 788 2243 均开放
                                 * 猜测应该是为了一些兼容。所有弹幕相关参数都有带上“dm_”前缀的趋势
                                 * */
                                ChatPort = int.Parse(doc["root"]["dm_port"].InnerText);
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        ChatHost = defaulthosts[new Random().Next(defaulthosts.Length)];

                        HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
                        if(errorResponse.StatusCode == HttpStatusCode.NotFound)
                        { // 直播间不存在（HTTP 404）
                            string msg = "该直播间疑似不存在，弹幕姬只支持使用原房间号连接";
                            LogMessage?.Invoke(this, new LogMessageArgs() { message = msg });
                        }
                        else
                        { // B站服务器响应错误
                            string msg = "B站服务器响应弹幕服务器地址出错，尝试使用常见地址连接";
                            LogMessage?.Invoke(this, new LogMessageArgs() { message = msg });
                        }
                    }
                    catch(Exception)
                    { // 其他错误（XML解析错误？）
                        ChatHost = defaulthosts[new Random().Next(defaulthosts.Length)];
                        string msg = "获取弹幕服务器地址时出现未知错误，尝试使用常见地址连接";
                        LogMessage?.Invoke(this, new LogMessageArgs() { message = msg });
                    }
                  

                }
                else
                {
                    ChatHost = lastserver;
                }
                Client = new TcpClient();

                await  Client.ConnectAsync(ChatHost, ChatPort);

                NetStream = Client.GetStream();


                if (SendJoinChannel(channelId))
                {
                    Connected = true;
                    this.HeartbeatLoop();
                    var thread = new Thread(this.ReceiveMessageLoop);
                    thread.IsBackground = true;
                    thread.Start();
                    lastserver = ChatHost;
                    lastroomid = roomId;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                this.Error = ex;
                return false;
            }
        }

        private void ReceiveMessageLoop()
        {
//            lock (shit_lock)
//            //ReceiveMessageLoop 似乎好像大概會同時運行兩個的bug, 但是不修了, 鎖上算了
//            {
                try
                {
                    var stableBuffer = new byte[Client.ReceiveBufferSize];

                    while (this.Connected)
                    {

                        NetStream.ReadB(stableBuffer, 0, 4);
                        var packetlength = BitConverter.ToInt32(stableBuffer, 0);
                        packetlength = IPAddress.NetworkToHostOrder(packetlength);

                        if (packetlength < 16)
                        {
                            throw new NotSupportedException("协议失败: (L:" + packetlength + ")");
                        }

                        NetStream.ReadB(stableBuffer, 0, 2);//magic
                        NetStream.ReadB(stableBuffer, 0, 2);//protocol_version 

                        NetStream.ReadB(stableBuffer, 0, 4);
                        var typeId = BitConverter.ToInt32(stableBuffer, 0);
                        typeId = IPAddress.NetworkToHostOrder(typeId);

                        Console.WriteLine(typeId);
                        NetStream.ReadB(stableBuffer, 0, 4);//magic, params?
                        var playloadlength = packetlength - 16;
                        if (playloadlength == 0)
                        {
                            continue;//没有内容了

                        }
                        typeId = typeId - 1;//和反编译的代码对应 
                        var buffer = new byte[playloadlength];
                        NetStream.ReadB(buffer, 0, playloadlength);
                        switch (typeId)
                        {
                            case 0:
                            case 1:
                            case 2:
                                {
                                    

                                    var viewer = BitConverter.ToUInt32(buffer.Take(4).Reverse().ToArray(), 0); //观众人数
                                    Console.WriteLine(viewer);
                                    if (ReceivedRoomCount != null)
                                    {
                                        ReceivedRoomCount(this, new ReceivedRoomCountArgs() { UserCount = viewer });
                                    }
                                    break;
                                }
                            case 3:
                            case 4://playerCommand
                                {
                                   
                                    var json = Encoding.UTF8.GetString(buffer, 0, playloadlength);
                                    if (debuglog)
                                    {
                                        Console.WriteLine(json);

                                    }
                                    try
                                    {
                                        DanmakuModel dama = new DanmakuModel(json, 2);
                                        if (ReceivedDanmaku != null)
                                        {
                                            ReceivedDanmaku(this, new ReceivedDanmakuArgs() { Danmaku = dama });
                                        }

                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }

                                    break;
                                }
                            case 5://newScrollMessage
                                {
                                    
                                    break;
                                }
                            case 7:
                                {
                                   
                                    break;
                                }
                            case 16:
                                {
                                    break;
                                }
                            default:
                                {
                                   
                                    break;
                                }
                                //                     
                        }
                    }
                }
                catch (NotSupportedException ex)
                {
                    this.Error = ex;
                    _disconnect();
                }
                catch (Exception ex)
                {
                    this.Error = ex;
                    _disconnect();

                }
//            }
            
        }

        private async void HeartbeatLoop()
        {

            try
            {
                while (this.Connected)
                {
                    this.SendHeartbeatAsync();
                    await TaskEx.Delay(30000);
                }
            }
            catch (Exception ex)
            {
                this.Error = ex;
                _disconnect();

            }
        }

        public void Disconnect()
        {

            Connected = false;
            try
            {
                Client.Close();
            }
            catch (Exception)
            {

            }


            NetStream = null;
        }

        private void _disconnect()
        {
            if (Connected)
            {
                Debug.WriteLine("Disconnected");

                Connected = false;

                Client.Close();

                NetStream = null;
                if (Disconnected != null)
                {
                    Disconnected(this, new DisconnectEvtArgs() {Error = Error});
                }
            }

        }

        private void SendHeartbeatAsync()
        {
            SendSocketData(2);
            Debug.WriteLine("Message Sent: Heartbeat");
        }

        void SendSocketData(int action, string body = "")
        {
            SendSocketData(0, 16, protocolversion, action, 1, body);
        }
        void SendSocketData(int packetlength, short magic, short ver, int action, int param = 1, string body = "")
        {
            var playload = Encoding.UTF8.GetBytes(body);
            if (packetlength == 0)
            {
                packetlength = playload.Length + 16;
            }
            var buffer = new byte[packetlength];
            using (var ms = new MemoryStream(buffer))
            {


                var b = BitConverter.GetBytes(buffer.Length).ToBE();

                ms.Write(b, 0, 4);
                b = BitConverter.GetBytes(magic).ToBE();
                ms.Write(b, 0, 2);
                b = BitConverter.GetBytes(ver).ToBE();
                ms.Write(b, 0, 2);
                b = BitConverter.GetBytes(action).ToBE();
                ms.Write(b, 0, 4);
                b = BitConverter.GetBytes(param).ToBE();
                ms.Write(b, 0, 4);
                if (playload.Length > 0)
                {
                    ms.Write(playload, 0, playload.Length);
                }
                NetStream.WriteAsync(buffer, 0, buffer.Length);
                NetStream.FlushAsync();
            }
        }

        private bool SendJoinChannel(int channelId)
        {
            
            Random r=new Random();
            var tmpuid = (long)(1e14 + 2e14*r.NextDouble());
            var packetModel = new {roomid = channelId, uid = tmpuid};
            var playload = JsonConvert.SerializeObject(packetModel);
            SendSocketData(7, playload);
            return true;
        }


        public DanmakuLoader()
        {
        }


    }

    public delegate void LogMessageEvt(object sender, LogMessageArgs e);
    public class LogMessageArgs
    {
        public string message = string.Empty;
    }                         
}