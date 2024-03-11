using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bililive_dm.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bililive_dm
{

    public class AnchorInfo
    {
        public int room_id { get; set; }
        public string uface { get; set; }
        public long uid { get; set; }
        public string uname { get; set; }
    }

    public class Data
    {
        public AnchorInfo anchor_info { get; set; }
        public GameInfo game_info { get; set; }
        public WebsocketInfo websocket_info { get; set; }
    }

    public class GameInfo
    {
        public string game_id { get; set; }
    }

    public class BOpenRoomInfo: BOpenApiResponse
    {
        public Data data { get; set; }
    }

    public class BOpenApiResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public string request_id { get; set; }
    }
    public class WebsocketInfo
    {
        public string auth_body { get; set; }
        public string[] wss_link { get; set; }
    }

    public struct RoomInfoData
    {
        public string auth;
        public string[] server;
        public int roomid;
        public string game_id;

    }

    public static class BOpen
    {
        private static HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };

        public static async Task<RoomInfoData> GetRoomInfoByCode(string code)
        {


            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_未輸入身份碼);
                }

                var param = JsonConvert.SerializeObject(new { code = code, app_id = 1651388990835 }, Formatting.None);
                var req = await httpClient.PostAsync("https://bopen.ceve-market.org/sign", new StringContent(param));
                if (!req.IsSuccessStatusCode)
                {
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_簽名伺服器離線);
                }

                var sign = JObject.Parse(await req.Content.ReadAsStringAsync());

                var req2 = new HttpRequestMessage(HttpMethod.Post, "https://live-open.biliapi.com/v2/app/start");
                req2.Content = new StringContent(param, Encoding.UTF8, "application/json");
                req2.Content.Headers.Remove("Content-Type"); // "{application/json; charset=utf-8}"
                req2.Content.Headers.Add("Content-Type", "application/json");
                foreach (var kv in sign)
                {
                    req2.Headers.Add(kv.Key, kv.Value + "");
                }
                req2.Headers.Add("Accept", "application/json");
                var resp = await httpClient.SendAsync(req2);
                if (!resp.IsSuccessStatusCode)
                {
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_B站直播中心離線);
                }
                var jo = JsonConvert.DeserializeObject<BOpenRoomInfo>(await resp.Content.ReadAsStringAsync());
              
                if (jo.code == 0)
                {
                    var roomid = jo.data?.anchor_info?.room_id;
                    if (roomid > 0 && !string.IsNullOrEmpty(jo?.data?.websocket_info?.auth_body))
                    {
                        return new RoomInfoData()
                        {
                            auth = jo?.data.websocket_info.auth_body,
                            server = jo.data.websocket_info.wss_link,
                            roomid = roomid.Value,
                            game_id = jo.data.game_info.game_id
                        };
                    }
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_B站直播中心返回了無效的房間號);

                }
                else
                {
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_B站直播中心返回錯誤+":"+jo.message);
                }

            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_獲取身份碼信息出錯, e);
            }
        }

        public static async Task<bool> HeartBeat(string gameid)
        {
            try
            {
                if (string.IsNullOrEmpty(gameid))
                {
                    return false;
                }

                var param = JsonConvert.SerializeObject(new { game_id = gameid }, Formatting.None);
                var req = await httpClient.PostAsync("https://bopen.ceve-market.org/sign", new StringContent(param));
                if (!req.IsSuccessStatusCode)
                {
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_簽名伺服器離線);
                }

                var sign = JObject.Parse(await req.Content.ReadAsStringAsync());

                var req2 = new HttpRequestMessage(HttpMethod.Post, "https://live-open.biliapi.com/v2/app/heartbeat");
                req2.Content = new StringContent(param, Encoding.UTF8, "application/json");
                req2.Content.Headers.Remove("Content-Type"); // "{application/json; charset=utf-8}"
                req2.Content.Headers.Add("Content-Type", "application/json");
                foreach (var kv in sign)
                {
                    req2.Headers.Add(kv.Key, kv.Value + "");
                }
                req2.Headers.Add("Accept", "application/json");
                var resp = await httpClient.SendAsync(req2);
                if (!resp.IsSuccessStatusCode)
                {
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_B站直播中心離線);
                }
                var jo = JsonConvert.DeserializeObject<BOpenApiResponse>(await resp.Content.ReadAsStringAsync());

                if (jo.code == 0)
                {
                    return true;

                }
                else
                {
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_B站直播中心返回錯誤 + ":" + jo.message);
                }

            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_獲取身份碼信息出錯, e);
            }
        }

    }
}