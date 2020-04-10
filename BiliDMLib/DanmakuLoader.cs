using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private int ChatPort = 2243; // TCP协议默认端口疑似修改到 2243
        private TcpClient Client;
        private NetworkStream NetStream;
        private string CIDInfoUrl = "https://api.live.bilibili.com/room/v1/Danmu/getConf?room_id=";
        private bool Connected = false;
        public Exception Error;
        public event ReceivedDanmakuEvt ReceivedDanmaku;
        public event DisconnectEvt Disconnected;
        public event ReceivedRoomCountEvt ReceivedRoomCount;
        public event LogMessageEvt LogMessage;
        private bool debuglog = true;
        private short protocolversion = 2;
        private static int lastroomid ;
        private static string lastserver;
        private static HttpClient httpClient=new HttpClient(){Timeout = TimeSpan.FromSeconds(5)};
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
                        var req = await httpClient.GetStringAsync(CIDInfoUrl + channelId);
                        var roomobj = JObject.Parse(req);

                        ChatHost = roomobj["data"]["host"]+"";

                        ChatPort = roomobj["data"]["port"].Value<int>();
                        if (string.IsNullOrEmpty(ChatHost))
                        {
                            throw new Exception();
                        }
                  
                    }
                    catch (WebException ex)
                    {
                        ChatHost = defaulthosts[new Random().Next(defaulthosts.Length)];

                        HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
                        if (errorResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            // 直播间不存在（HTTP 404）
                            string msg = "该直播间疑似不存在，弹幕姬只支持使用原房间号连接";
                            LogMessage?.Invoke(this, new LogMessageArgs() {message = msg});
                        }
                        else
                        {
                            // B站服务器响应错误
                            string msg = "B站服务器响应弹幕服务器地址出错，尝试使用常见地址连接";
                            LogMessage?.Invoke(this, new LogMessageArgs() {message = msg});
                        }
                    }
                    catch (Exception)
                    {
                        // 其他错误（XML解析错误？）
                        ChatHost = defaulthosts[new Random().Next(defaulthosts.Length)];
                        string msg = "获取弹幕服务器地址时出现未知错误，尝试使用常见地址连接";
                        LogMessage?.Invoke(this, new LogMessageArgs() {message = msg});
                    }


                }
                else
                {
                    ChatHost = lastserver;
                }
                Client = new TcpClient();

                var ipaddrss = await System.Net.Dns.GetHostAddressesAsync(ChatHost);
                var random = new Random();
                var idx=random.Next(ipaddrss.Length);
                await  Client.ConnectAsync(ipaddrss[idx], ChatPort);

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
                var stableBuffer = new byte[16];
                var buffer = new byte[4096];
                while (this.Connected)
                {
                    NetStream.ReadB(stableBuffer, 0, 16);
                    Parse2Protocol(stableBuffer, out DanmakuProtocol protocol);
                    if (protocol.PacketLength < 16)
                    {
                        throw new NotSupportedException("协议失败: (L:" + protocol.PacketLength + ")");
                    }
                    var payloadlength = protocol.PacketLength - 16;
                    if (payloadlength == 0)
                    {
                        continue; // 没有内容了
                    }
                    if (buffer.Length < payloadlength) // 不够长再申请
                    {
                        buffer = new byte[payloadlength];
                    }
                    NetStream.ReadB(buffer, 0, payloadlength);
                    if (protocol.Version == 2 && protocol.Action == 5) // 处理deflate消息
                    {
                        using (MemoryStream ms = new MemoryStream(buffer, 2, payloadlength - 2)) // Skip 0x78 0xDA
                        using (DeflateStream deflate = new DeflateStream(ms, CompressionMode.Decompress))
                        {
                            while (deflate.Read(stableBuffer, 0, 16) > 0)
                            {
                                Parse2Protocol(stableBuffer, out protocol);
                                payloadlength = protocol.PacketLength - 16;
                                if (payloadlength == 0)
                                {
                                    continue; // 没有内容了
                                }
                                if (buffer.Length < payloadlength) // 不够长再申请
                                {
                                    buffer = new byte[payloadlength];
                                }
                                deflate.Read(buffer, 0, payloadlength);
                                ProcessDanmaku(protocol.Action, buffer, payloadlength);
                            }
                        }
                    }
                    else
                    {
                        ProcessDanmaku(protocol.Action, buffer, payloadlength);
                    }
                }
            }
            //catch (NotSupportedException ex)
            //{
            //    this.Error = ex;
            //    _disconnect();
            //}
            catch (Exception ex)
            {
                this.Error = ex;
                _disconnect();

            }
//            }
            
        }

        private  void ProcessDanmaku(int action, byte[] buffer, int length)
        {
            switch (action)
            {
                case 3: // (OpHeartbeatReply)
                    {
                        var viewer = BitConverter.ToUInt32(buffer.Take(4).Reverse().ToArray(), 0); //观众人数
                        Console.WriteLine(viewer);
                        if (ReceivedRoomCount != null)
                        {
                            ReceivedRoomCount(this, new ReceivedRoomCountArgs() { UserCount = viewer });
                        }
                        break;
                    }
                case 5://playerCommand (OpSendMsgReply)
                    {

                        var json = Encoding.UTF8.GetString(buffer, 0, length);
                        if (debuglog)
                        {
                            Console.WriteLine(json);
                        }
                        try
                        {
                            DanmakuModel dama = new DanmakuModel(json, 2);
                            ReceivedDanmaku?.Invoke(this, new ReceivedDanmakuArgs() { Danmaku = dama });
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        break;
                    }
                case 8: // (OpAuthReply)
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private async void HeartbeatLoop()
        {

            try
            {
                while (this.Connected)
                {
                    this.SendHeartbeatAsync();
                    await Task.Delay(30000);
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
            var packetModel = new {roomid = channelId, uid = tmpuid, protover = 2};
            var playload = JsonConvert.SerializeObject(packetModel);
            SendSocketData(7, playload);
            return true;
        }

        private static unsafe void Parse2Protocol(byte[] buffer, out DanmakuProtocol protocol)
        {
            fixed (byte* ptr = buffer)
            {
                protocol = *(DanmakuProtocol*)ptr;
            }
            protocol.ChangeEndian();
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


    public struct DanmakuProtocol
    {
        /// <summary>
        /// 消息总长度 (协议头 + 数据长度)
        /// </summary>
        public int PacketLength;
        /// <summary>
        /// 消息头长度 (固定为16[sizeof(DanmakuProtocol)])
        /// </summary>
        public short HeaderLength;
        /// <summary>
        /// 消息版本号
        /// </summary>
        public short Version;
        /// <summary>
        /// 消息类型
        /// </summary>
        public int Action;
        /// <summary>
        /// 参数, 固定为1
        /// </summary>
        public int Parameter;
        /// <summary>
        /// 转为本机字节序
        /// </summary>
        public void ChangeEndian()
        {
            PacketLength = IPAddress.HostToNetworkOrder(PacketLength);
            HeaderLength = IPAddress.HostToNetworkOrder(HeaderLength);
            Version = IPAddress.HostToNetworkOrder(Version);
            Action = IPAddress.HostToNetworkOrder(Action);
            Parameter = IPAddress.HostToNetworkOrder(Parameter);
        }
    }

}