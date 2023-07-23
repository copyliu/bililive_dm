using System.Buffers;
using System.Diagnostics;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Microsoft.Toolkit.HighPerformance;
using Newtonsoft.Json;

namespace BiliDMLibCore;

public class RoomConnecter
{
    private static readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

    private static readonly string
        CIDInfoUrl = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo?id=";

    public static async Task<DanmakuTcpConnection> ConnectAsync(int roomId)
    {
        if (roomId > 0)
            try
            {
                var info = await httpClient.GetFromJsonAsync<DanmuInfo>(CIDInfoUrl + roomId);

                var token = info.data.token;

                foreach (var serverinfo in info.data.host_list)
                {
                    DanmakuTcpConnection dammaku = null;
                    try
                    {
                        dammaku = new DanmakuTcpConnection(serverinfo.host, serverinfo.port, roomId, token);
                        var _ = dammaku.ConnectAsync(CancellationToken.None);
                        return dammaku;
                    }
                    catch (Exception e)
                    {
                        dammaku?.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
            }

        throw new Exception();
    }

    private class DanmuInfo
    {
        public Data data { get; }

        public class Data
        {
            public string token { get; }
            public List<Host> host_list { get; }
        }

        public class Host
        {
            public string host { get; }
            public ushort port { get; }
            public ushort wss_port { get; set; }
            public ushort ws_port { get; set; }
        }
    }
}

public class DanmakuTcpConnection : IDisposable
{
    private readonly TcpClient _client;
    private readonly Pipe _pipe = new();
    private readonly int _port;
    private readonly int _roomId;
    private readonly string _server;
    private readonly string _token;
    private Task? _readLoop;

    public BufferBlock<DanmakuModel> DanmakuSource = new(new DataflowBlockOptions { EnsureOrdered = true });

    private Task? heartbeatLoop;
    private readonly short protocolversion = 1;

    public DanmakuTcpConnection(string server, int port, int roomId, string token)
    {
        _server = server;
        _port = port;
        _roomId = roomId;
        _token = token;
        _client = new TcpClient();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _readLoop?.Dispose();
        _client.Dispose();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (_client.Connected == false && _readLoop == null)
        {
            await _client.ConnectAsync(_server, _port, cancellationToken);
            if (await SendJoinChannel(_roomId, _token, cancellationToken))
            {
                _readLoop = ExecuteLoop(cancellationToken);
                await _readLoop;
            }
        }

        else if (_readLoop != null)
        {
            await _readLoop;
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    private async Task FillPipeAsync(Socket socket, PipeWriter writer, CancellationToken cancellationToken)
    {
        const int minimumBufferSize = 512;

        while (true)
        {
            // Allocate at least 512 bytes from the PipeWriter.
            var memory = writer.GetMemory(minimumBufferSize);
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
                var bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, linked.Token).ConfigureAwait(false);
                if (bytesRead == 0) break;

                // Tell the PipeWriter how much was read from the Socket.
                writer.Advance(bytesRead);
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception ex)
            {
                break;
            }

            // Make the data available to the PipeReader.
            var result = await writer.FlushAsync(cancellationToken);

            if (result.IsCompleted) break;
        }

        // By completing PipeWriter, tell the PipeReader that there's no more data coming.

        await writer.CompleteAsync();
    }

    private async Task ReadPipeAsync(PipeReader reader, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
            try
            {
                var result = await reader.ReadAsync(ct);

                var buffer = result.Buffer;
                var header = DanmakuProtocol.FromBuffer(buffer.Slice(0, 16));
                if (header == null) continue;

                if (buffer.Length < header.Value.PacketLength) continue;

                var protocol = header.Value;
                heartbeatLoop ??= HeartbeatLoop(ct);

                switch (protocol.Version)
                {
                    // 处理deflate消息
                    case 2 when protocol.Action == 5:
                    {
                        var data = buffer.Slice(16 + 2, header.Value.PacketLength - 16 - 2).ToArray();
                        var memory =
                            new ReadOnlyMemory<byte>(
                                data); //.NET 7 之后要改 //https://github.com/dotnet/runtime/issues/58216

                        await using var deflate = new DeflateStream(memory.AsStream(), CompressionMode.Decompress);
                        var headerbuffer = new byte[16];
                        try
                        {
                            while (true)
                            {
                                if (await deflate.ReadAsync(headerbuffer, ct) != 16) throw new Exception();

                                var protocol_in = DanmakuProtocol.FromBuffer(new ReadOnlySequence<byte>(headerbuffer));
                                if (protocol_in == null) throw new Exception();

                                var payloadlength = protocol_in.Value.PacketLength - 16;

                                var danmakubuffer = new byte[payloadlength];
                                if (await deflate.ReadAsync(danmakubuffer, ct) != payloadlength) throw new Exception();

                                ;
                                var r = ProcessDanmaku(protocol.Action, new ReadOnlySequence<byte>(danmakubuffer));
                                if (r != null) DanmakuSource.Post(r);
                            }
                        }
                        catch (Exception e)
                        {
                        }

                        break;
                    }
                    // brotli?
                    case 3 when protocol.Action == 5:
                    {
                        var data = buffer.Slice(16, header.Value.PacketLength - 16).ToArray();
                        var memory = new ReadOnlyMemory<byte>(data); //.NET 7 之后要改
                        await using var deflate = new BrotliStream(memory.AsStream(), CompressionMode.Decompress);
                        var headerbuffer = new byte[16];
                        try
                        {
                            while (true)
                            {
                                if (await deflate.ReadAsync(headerbuffer, ct) != 16) throw new Exception();

                                var protocol_in = DanmakuProtocol.FromBuffer(new ReadOnlySequence<byte>(headerbuffer));
                                if (protocol_in == null) throw new Exception();

                                var payloadlength = protocol_in.Value.PacketLength - 16;

                                var danmakubuffer = new byte[payloadlength];
                                if (await deflate.ReadAsync(danmakubuffer, ct) != payloadlength) throw new Exception();

                                ;
                                var r = ProcessDanmaku(protocol.Action, new ReadOnlySequence<byte>(danmakubuffer));
                                if (r != null) DanmakuSource.Post(r);
                            }
                        }
                        catch (Exception e)
                        {
                        }

                        break;
                    }
                    default:
                    {
                        var r = ProcessDanmaku(protocol.Action, buffer.Slice(16));
                        if (r != null) DanmakuSource.Post(r);
                    }
                        break;
                }

                reader.AdvanceTo(buffer.Slice(protocol.PacketLength).Start);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch
            {
                break;
            }

        await reader.CompleteAsync();
        _client.Close();
    }

    private async Task ExecuteLoop(CancellationToken cancellationToken)
    {
        var writing = FillPipeAsync(_client.Client, _pipe.Writer, cancellationToken);
        var reading = ReadPipeAsync(_pipe.Reader, cancellationToken);
        await Task.WhenAll(reading, writing);
        if (_client.Connected) _client.Close();
    }

    private async Task HeartbeatLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (_client.Connected)
            {
                await SendHeartbeatAsync(cancellationToken);
                await Task.Delay(30000, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _disconnect();
        }
    }

    private async Task SendHeartbeatAsync(CancellationToken ct)
    {
        await SendSocketDataAsync(2, "[object Object]", ct);
        Debug.WriteLine("Message Sent: Heartbeat");
    }

    private Task SendSocketDataAsync(int action, string body, CancellationToken ct)
    {
        return SendSocketDataAsync(0, 16, protocolversion, action, 1, body, ct);
    }

    private async Task SendSocketDataAsync(int packetlength, short magic, short ver, int action, int param, string body,
        CancellationToken ct)
    {
        var playload = Encoding.UTF8.GetBytes(body);
        if (packetlength == 0) packetlength = playload.Length + 16;

        var buffer = new byte[packetlength];
        using var ms = new MemoryStream(buffer);
        var b = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(buffer.Length));

        await ms.WriteAsync(b, ct);
        b = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(magic));
        await ms.WriteAsync(b, ct);
        b = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ver));
        await ms.WriteAsync(b, ct);
        b = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(action));
        await ms.WriteAsync(b, ct);
        b = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(param));
        await ms.WriteAsync(b, ct);
        if (playload.Length > 0) await ms.WriteAsync(playload, ct);

        await _client.Client.SendAsync(buffer, SocketFlags.None);
    }

    private async Task<bool> SendJoinChannel(int channelId, string token, CancellationToken ct)
    {
        var packetModel = new
            { roomid = channelId, uid = 0, protover = 3, key = token, platform = "danmuji", type = 2 };


        var playload = JsonConvert.SerializeObject(packetModel);
        await SendSocketDataAsync(7, playload, ct);
        return true;
    }

    private void _disconnect()
    {
        if (_client.Connected)
        {
            Debug.WriteLine("Disconnected");
            // cancellationTokenSource.Cancel();


            _client.Close();
            //     
            // NetStream = null;
            // if (Disconnected != null)
            // {
            //     Disconnected(this, new DisconnectEvtArgs() {Error = Error});
            // }
        }
    }

    public async Task Stop()
    {
    }

    private DanmakuModel? ProcessDanmaku(int action, ReadOnlySequence<byte> buffer)
    {
        var debuglog = false;
        var reader = new SequenceReader<byte>(buffer);
        switch (action)
        {
            case 3: // (OpHeartbeatReply)
            {
                if (reader.TryReadBigEndian(out int viewer))
                {
                    // Console.WriteLine(viewer);
                    // ReceivedRoomCount?.Invoke(this, new ReceivedRoomCountArgs() { UserCount = viewer });
                }

                break;
            }
            case 5: //playerCommand (OpSendMsgReply)
            {
                var json = Encoding.UTF8.GetString(buffer);
                if (debuglog) Console.WriteLine(json);

                try
                {
                    return new DanmakuModel(json, 2);
                    // ReceivedDanmaku?.Invoke(this, new ReceivedDanmakuArgs() { Danmaku = dama });
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

        return null;
    }
}