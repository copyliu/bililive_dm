using System;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;

namespace Bililive_dm
{
    public class Live2DViewerExProtocol
    {
        static int _id = 100000;
        public static void SendMsg(string msg)
        {
            try
            {
                var canceltoken=new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using (var client = new ClientWebSocket())
                {
                    client.ConnectAsync(new Uri("ws://127.0.0.1:10086/api"), canceltoken.Token).Wait(canceltoken.Token);
                    var data=JsonConvert.SerializeObject(new
                    {
                        msg = 11000,
                        id = _id++,
                        data = new
                        {
                            id = 0,
                            text = msg,
                            duration = 5000
                        }
                    });
                    client.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(data)), WebSocketMessageType.Text, true, canceltoken.Token).Wait(canceltoken.Token);
                   
                }
            }
            catch (Exception e)
            {
             
            }
        }
    }
}