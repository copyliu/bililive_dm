using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bililive_dm.Properties;

namespace Bililive_dm
{
    public static class BOpen
    {
        private static HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
        public static async Task<int> GetRoomIdByCode(string code)
        {


            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_未輸入身份碼);
                }

                var param = JsonConvert.SerializeObject(new { code = code,app_id= 1651388990835 }, Formatting.None);
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
                req2.Headers.Add("Accept","application/json");
                var resp = await httpClient.SendAsync(req2);
                if (!resp.IsSuccessStatusCode)
                {
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_B站直播中心離線);
                }
                var jo = JObject.Parse(await resp.Content.ReadAsStringAsync());
                if (jo.Value<int>("code") == 0)
                {
                    var roomid = jo["data"]["anchor_info"].Value<int>("room_id");
                    if (roomid > 0)
                    {
                        return roomid;
                    }
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_B站直播中心返回了無效的房間號);

                }
                else
                {
                    throw new NotSupportedException(Resources.BOpen_GetRoomIdByCode_B站直播中心返回錯誤);
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
