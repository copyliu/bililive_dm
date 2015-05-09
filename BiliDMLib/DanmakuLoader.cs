using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace BiliDMLib
{
    public class DanmakuLoader
    {
        private string ChatHost = "chat.bilibili.com";
        private int ChatPort = 88;
        private TcpClient Client;
        private NetworkStream NetStream;
//        private string RoomInfoUrl = "http://live.bilibili.com/sch_list/";
        private string CIDInfoUrl = "http://live.bilibili.com/api/player?id=cid:";
        private bool Connected = false;
        public Exception Error;
        public event ReceivedDanmakuEvt ReceivedDanmaku;
        public event DisconnectEvt Disconnected;
        public event ReceivedRoomCountEvt ReceivedRoomCount;

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


                var request2 = WebRequest.Create(CIDInfoUrl + channelId);
                var response2 = request2.GetResponse();
                using (var stream = response2.GetResponseStream())
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var text = await sr.ReadToEndAsync();
                        var xml = "<root>" + text + "</root>";
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xml);
                        ChatHost = doc["root"]["server"].InnerText;
                    }
                }


                Client = new TcpClient();

                Client.Connect(ChatHost, ChatPort);

                NetStream = Client.GetStream();


                if (SendJoinChannel(channelId))
                {
                    Connected = true;
                    this.HeartbeatLoop();
                    var thread = new Thread(this.ReceiveMessageLoop);
                    thread.IsBackground = true;
                    thread.Start();

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
            try
            {
                var stableBuffer = new byte[Client.ReceiveBufferSize];

                while (this.Connected)
                {
                    NetStream.Read(stableBuffer, 0, 2);
                    var typeId = BitConverter.ToInt16(stableBuffer, 0);
                    typeId = IPAddress.NetworkToHostOrder(typeId);
                    Console.WriteLine(typeId);
                    switch (typeId)
                    {
                        case 1:
                            NetStream.Read(stableBuffer, 0, 4);
                            var viewer = BitConverter.ToInt32(stableBuffer, 0);
                            viewer = IPAddress.NetworkToHostOrder(viewer); //觀眾人數
                            if (ReceivedRoomCount != null)
                            {
                                ReceivedRoomCount(this, new ReceivedRoomCountArgs() {UserCount = viewer});
                            }

                            break;
                        case 2: //newCommentString
                        {
                            NetStream.Read(stableBuffer, 0, 2);
                            var packetlength = BitConverter.ToInt16(stableBuffer, 0);
                            packetlength = (short) (IPAddress.NetworkToHostOrder(packetlength) - 4);

                            var buffer = new byte[packetlength];

                            NetStream.Read(buffer, 0, packetlength);
                            var json = Encoding.UTF8.GetString(buffer, 0, packetlength);
                            DanmakuModel dama = new DanmakuModel(json);
//                            Console.WriteLine(dama.CommentText);
                            if (ReceivedDanmaku != null)
                            {
                                ReceivedDanmaku(this, new ReceivedDanmakuArgs() {Danmaku = dama});
                            }

                            break;
                        }
                        case 8: //x 条新的评论信息
                        {
                            NetStream.Read(stableBuffer, 0, 2);

                            break;
                        }
                        case 17: //_playerDebug
                        {
                            //server updated
                            break;
                        }
                        case 4: //playerCommand
                        {
                            NetStream.Read(stableBuffer, 0, 2);
                            var packetlength = BitConverter.ToInt16(stableBuffer, 0);
                            packetlength = (short) (IPAddress.NetworkToHostOrder(packetlength) - 4);

                            var buffer = new byte[packetlength];

                            NetStream.Read(buffer, 0, packetlength);
                            var json = Encoding.UTF8.GetString(buffer, 0, packetlength);
                            try
                            {
                                DanmakuModel dama = new DanmakuModel(json, 2);
                                if (ReceivedDanmaku != null)
                                {
                                    ReceivedDanmaku(this, new ReceivedDanmakuArgs() {Danmaku = dama});
                                }

                            }
                            catch (Exception)
                            {
                                // ignored
                            }

                            break;
                        }
                        case 5: //playerBroadcast
                        case 6: //newScrollMessage
                        default:
                        {
                            NetStream.Read(stableBuffer, 0, 2);
                            var packetlength = BitConverter.ToInt16(stableBuffer, 0);
                            packetlength = (short) (IPAddress.NetworkToHostOrder(packetlength) - 4);

                            var buffer = new byte[packetlength];

                            NetStream.Read(buffer, 0, packetlength);

                            break;
                        }

//                        
//                            Disconnect();
//                            this.Error = new Exception("收到非法包");
//                            break;
                    }
                }
            }
            catch (NotSupportedException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Disconnect();
                this.Error = ex;
            }
        }

        private async void HeartbeatLoop()
        {
            
            try
            {
                while (this.Connected)
                {
                    this.SendHeartbeatAsync();
                    await TaskEx.Delay(60000);
                }
            }
            catch
            {
                this.Disconnect();
                throw;
            }
        }

        private void Disconnect()
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

        private void SendHeartbeatAsync()
        {
            var buffer = BitConverter.GetBytes(16908292).ToBE(); //0x01,0x02,0x00,0x04

            NetStream.WriteAsync(buffer, 0, buffer.Length);
            NetStream.FlushAsync();
            Debug.WriteLine("Message Sent: Heartbeat");
        }

        private bool SendJoinChannel(int channelId)
        {
            var buffer = new byte[12];

            using (var ms = new MemoryStream(buffer))
            {
                var b = BitConverter.GetBytes(16842764).ToBE(); //0x01,0x01,0x00,0x0C

                ms.Write(b, 0, 4);
                b = BitConverter.GetBytes(channelId).ToBE();
                ms.Write(b, 0, 4);
                b = BitConverter.GetBytes(0).ToBE();
                ms.Write(b, 0, 4);
                NetStream.WriteAsync(buffer, 0, buffer.Length);
                NetStream.FlushAsync();

                Debug.WriteLine("Message Sent: Join Channel");
            }

            return true;
        }


        public DanmakuLoader()
        {
        }


    }
}