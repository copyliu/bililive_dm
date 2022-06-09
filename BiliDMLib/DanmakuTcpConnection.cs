using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BilibiliDM_PluginFramework;

namespace BiliDMLib
{
    public class DanmakuTcpConnection
    {
        private readonly string _token;

        private readonly Pipe _pipe;
        public BufferBlock<DanmakuModel> DanmakuSource =
            new BufferBlock<DanmakuModel>(new DataflowBlockOptions() { EnsureOrdered = true });
        private readonly TcpClient _client;
        public DanmakuTcpConnection(string server,int port,string token)
        {
            _token = token;
            _client = new TcpClient(server, port);
        }
        async Task FillPipeAsync(Socket socket, PipeWriter writer, CancellationToken cancellationToken)
        {
            const int minimumBufferSize = 512;

            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter.
                Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                    using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, linked.Token).ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    // Tell the PipeWriter how much was read from the Socket.
                    writer.Advance(bytesRead);
                }
                catch (SocketException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "出现错误");
                    break;
                }

                // Make the data available to the PipeReader.
                FlushResult result = await writer.FlushAsync(cancellationToken);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // By completing PipeWriter, tell the PipeReader that there's no more data coming.

            await writer.CompleteAsync();
        }
        private async Task ReadPipeAsync(PipeReader reader, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await reader.ReadAsync(cancellationToken);

                    var buffer = result.Buffer;
                    if (buffer.Length > 0)
                    {
                        switch (buffer.FirstSpan[0])
                        {
                            case 0x00:
                                await _client.Client.SendAsync(result.Buffer.Slice(0, 1).First, SocketFlags.None,
                                    cancellationToken);
                                buffer = buffer.Slice(1);
                                reader.AdvanceTo(buffer.Start);
                                break;
                            case 0xD5:
                                if (UploadDataFrame.TryParse(ref buffer, out var frame))
                                {
                                    reader.AdvanceTo(buffer.Start);
                                    await ProcessDeviceMsg(frame);
                                }
                                else
                                {
                                    reader.AdvanceTo(buffer.Start, buffer.End);
                                }

                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch
                {
                    break;
                }
            }

            await reader.CompleteAsync();
            _client.Close();
        }

        public async Task ExecuteLoop(CancellationToken cancellationToken)
        {
            Task writing = FillPipeAsync(_client.Client, _pipe.Writer, cancellationToken);
            Task reading = ReadPipeAsync(_pipe.Reader, cancellationToken);
            var r = _client.GetStream()
            r.Read()
            await Task.WhenAll(reading, writing);
            if (_client.Connected)
            {
                _client.Close();
            }

            _logger.LogInformation("链接完成");
        }
        public async Task Start()
        {
            using var clientcts = new CancellationTokenSource();
            
        }

        public async Task Stop()
        {
            
        }
        
        
        
        
    }
}