using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BilibiliDM_PluginFramework;
using BitConverter;
using Brotli;

namespace BiliDMLib
{
    public class OpenDanmakuLoader:IDisposable
    {
        private readonly string _auth;
        private readonly string[] _server;
        private TcpClient _client;
        private Stream NetStream;
        private readonly int defaultport = 2243;
        
        public event ReceivedDanmakuEvt ReceivedDanmaku;
        public event DisconnectEvt Disconnected;
        public event ReceivedRoomCountEvt ReceivedRoomCount;
        public event LogMessageEvt LogMessage;
        private readonly bool debuglog = true;
        public bool Connected { get; private set; }
        public string GameId { get; set; }
        
        private CancellationTokenSource cancellationTokenSource;
        private Timer PlatformHeartBeatTimer;

        public void PlatformHeartBeatOk()
        {
            PlatformHeartBeatTimer?.Change(TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);
        }

        public void Disconnect()
        {
            Connected = false;
            try
            {
                _client.Close();
            }
            catch (Exception)
            {
            }

            GameId = null;
            NetStream = null;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (Connected) throw new InvalidOperationException();


                var server = new List<Uri>();
                foreach (var s in _server)
                {
                    if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var uri))
                    {
                        server.Add(uri);
                    }
                }



                _client = new TcpClient();
                var random = new Random();
                var idx = random.Next(server.Count);
                await _client.ConnectAsync(server[idx].Host, defaultport);

                NetStream = Stream.Synchronized(_client.GetStream());
                cancellationTokenSource = new CancellationTokenSource();

                if (await SendJoinChannel(cancellationTokenSource.Token))
                {
                    Connected = true;
                    _ = ReceiveMessageLoop(cancellationTokenSource.Token);
                     PlatformHeartBeatTimer = new Timer(state => cancellationTokenSource.Cancel(),null,TimeSpan.FromMinutes(1), System.Threading.Timeout.InfiniteTimeSpan);
                     
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void _disconnect(Exception ex)
        {
            if (Connected)
            {
                Debug.WriteLine("Disconnected");
                cancellationTokenSource.Cancel();

                Connected = false;

                _client.Close();

                NetStream = null;
                GameId = null;
                Disconnected?.Invoke(this, new DisconnectEvtArgs { Error = ex });
            }
        }
        
        private async Task ReceiveMessageLoop(CancellationToken ct)
        {
            try
            {
                _ = HeartbeatLoop(cancellationTokenSource.Token);
                var stableBuffer = new byte[16];
                var buffer = new byte[4096];
                while (Connected)
                {
                    await NetStream.ReadBAsync(stableBuffer, 0, 16, ct);
                    var protocol = DanmakuProtocol.FromBuffer(stableBuffer);
                    if (protocol.PacketLength < 16)
                        throw new NotSupportedException("协议失败: (L:" + protocol.PacketLength + ")");
                    var payloadlength = protocol.PacketLength - 16;
                    if (payloadlength == 0) continue; // 没有内容了

                    buffer = new byte[payloadlength];

                    await NetStream.ReadBAsync(buffer, 0, payloadlength, ct);
                    
                    if (protocol.Version == 2 && protocol.Action == 5) // 处理deflate消息
                        using (var ms = new MemoryStream(buffer, 2, payloadlength - 2)) // Skip 0x78 0xDA
                        using (var deflate = new DeflateStream(ms, CompressionMode.Decompress))
                        {
                            var headerbuffer = new byte[16];
                            try
                            {
                                while (true)
                                {
                                    await deflate.ReadBAsync(headerbuffer, 0, 16, ct);
                                    var protocol_in = DanmakuProtocol.FromBuffer(headerbuffer);
                                    payloadlength = protocol_in.PacketLength - 16;
                                    var danmakubuffer = new byte[payloadlength];
                                    await deflate.ReadBAsync(danmakubuffer, 0, payloadlength, ct);
                                    ProcessDanmaku(protocol.Action, danmakubuffer);
                                }
                            }
                            catch (Exception e)
                            {
                            }
                        }
                    else if (protocol.Version == 3 && protocol.Action == 5) // brotli?
                        using (var ms = new MemoryStream(buffer)) // Skip 0x78 0xDA

                        using (var deflate = new BrotliStream(ms, CompressionMode.Decompress))
                        {
                            var headerbuffer = new byte[16];
                            try
                            {
                                while (true)
                                {
                                    await deflate.ReadBAsync(headerbuffer, 0, 16, ct);
                                    var protocol_in = DanmakuProtocol.FromBuffer(headerbuffer);
                                    payloadlength = protocol_in.PacketLength - 16;
                                    var danmakubuffer = new byte[payloadlength];
                                    await deflate.ReadBAsync(danmakubuffer, 0, payloadlength, ct);
                                    ProcessDanmaku(protocol.Action, danmakubuffer);
                                }
                            }
                            catch (Exception e)
                            {
                            }
                        }
                    else
                        ProcessDanmaku(protocol.Action, buffer);
                }
            }
            //catch (NotSupportedException ex)
            //{
            //    this.Error = ex;
            //    _disconnect();
            //}
            catch (Exception ex)
            { 
                _disconnect(ex);
            }
        }

        private void ProcessDanmaku(int action, byte[] buffer)
        {
            switch (action)
            {
                case 3: // (OpHeartbeatReply)
                {
                    var viewer = EndianBitConverter.BigEndian.ToUInt32(buffer, 0); //观众人数
                    // Console.WriteLine(viewer);
                    ReceivedRoomCount?.Invoke(this, new ReceivedRoomCountArgs { UserCount = viewer });
                    break;
                }
                case 5: //playerCommand (OpSendMsgReply)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    if (debuglog) Console.WriteLine(json);
                    try
                    {
                        var dama = new DanmakuModel(json, 2);
                        ReceivedDanmaku?.Invoke(this, new ReceivedDanmakuArgs { Danmaku = dama });
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
            }
        }

        private async Task HeartbeatLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (Connected)
                {
                    await SendHeartbeatAsync(cancellationToken);
                    await Task.Delay(30000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _disconnect(ex);
            }
        }

    

        private async Task SendHeartbeatAsync(CancellationToken ct)
        {
            await SendSocketDataAsync(2, _auth, ct);
            Debug.WriteLine("Message Sent: Heartbeat");
        }

        private Task SendSocketDataAsync(int action, string body, CancellationToken ct)
        {
            return SendSocketDataAsync(0, 16, 3, action, 1, body, ct);
        }

        private async Task SendSocketDataAsync(int packetlength, short magic, short ver, int action, int param,
            string body, CancellationToken ct)
        {
            var playload = Encoding.UTF8.GetBytes(body);
            if (packetlength == 0) packetlength = playload.Length + 16;
            var buffer = new byte[packetlength];
            using (var ms = new MemoryStream(buffer))
            {
                var b = EndianBitConverter.BigEndian.GetBytes(buffer.Length);

                await ms.WriteAsync(b, 0, 4);
                b = EndianBitConverter.BigEndian.GetBytes(magic);
                await ms.WriteAsync(b, 0, 2);
                b = EndianBitConverter.BigEndian.GetBytes(ver);
                await ms.WriteAsync(b, 0, 2);
                b = EndianBitConverter.BigEndian.GetBytes(action);
                await ms.WriteAsync(b, 0, 4);
                b = EndianBitConverter.BigEndian.GetBytes(param);
                await ms.WriteAsync(b, 0, 4);
                if (playload.Length > 0) await ms.WriteAsync(playload, 0, playload.Length);
                await NetStream.WriteAsync(buffer, 0, buffer.Length, ct);
            }
        }

        private async Task<bool> SendJoinChannel(CancellationToken ct)
        {
          
            await SendSocketDataAsync(7, _auth, ct);
            return true;
        }
        
        public OpenDanmakuLoader(string auth, string[] server,string gameId)
        {
            _auth = auth;
            _server = server;
            GameId=gameId;
            

        }


        
        
        
        
        /// <inheritdoc />
        public void Dispose()
        {
            _client?.Dispose();
            NetStream?.Dispose();
        }

        public void ForceDisconnect()
        {
            this.cancellationTokenSource.Cancel();
        }
    }
}
