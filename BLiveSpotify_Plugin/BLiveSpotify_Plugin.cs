using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BilibiliDM_PluginFramework;
using BLiveSpotify_Plugin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BLiveSpotify_Plugin
{
    public class MyException : Exception
    {
        public MyException(string msg) : base(msg)
        {
        }
    }

    public class SpotifyLib
    {
        private string access_token;
        private readonly HttpClient client = new HttpClient();
        private DateTime expriedate;
        private readonly HttpListener listener = new HttpListener();

        public string refresh_token { get; set; }
        public string playdevice { get; set; }

        private async Task<bool> update_token()
        {
            if (string.IsNullOrWhiteSpace(refresh_token)) return false;

            if (expriedate < DateTime.Now || string.IsNullOrEmpty(access_token))
                try
                {
                    var nv = HttpUtility.ParseQueryString(string.Empty);
                    nv["grant_type"] = "refresh_token";
                    nv["refresh_token"] = refresh_token;
                    nv["client_id"] = BLiveSpotify_Plugin.APPID;
                    var result = await client.PostAsync("https://accounts.spotify.com/api/token",
                        new StringContent(nv.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded"));
                    if (result.IsSuccessStatusCode)
                    {
                        var res = await result.Content.ReadAsStringAsync();
                        var jobj = JObject.Parse(res);
                        access_token = jobj["access_token"] + "";
                        expriedate = DateTime.Now.AddSeconds(jobj.Value<int>("expires_in"));
                        refresh_token = jobj["refresh_token"] + "";
                        SaveConfig();
                        return true;
                    }

                    refresh_token = "";
                    SaveConfig();
                    return false;
                }
                catch (Exception e)
                {
                    refresh_token = "";
                    SaveConfig();
                    return false;
                }

            return true;
        }


        public async Task<List<PlayDeviceModel>> GetPlayDevices()
        {
            if (await update_token())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/devices");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var txt = await result.Content.ReadAsStringAsync();
                    var jobj = JsonConvert.DeserializeObject<PlayDeviceResponse>(txt);
                    var results = jobj.devices.Select(p => new PlayDeviceModel
                    {
                        PlaylistId = p.id,
                        PlaylistName = p.name
                    }).ToList();


                    return results;
                }

                AddLog(await result.Content.ReadAsStringAsync());
                throw new MyException("獲取播放機失敗");
            }

            throw new MyException("授權失效");
        }

        public async Task<MusicModel> SearchMusic(string name)
        {
            if (await update_token())
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://api.spotify.com/v1/search?type=track&include_external=audio&limit=1&q={HttpUtility.UrlEncode(name)}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var txt = await result.Content.ReadAsStringAsync();
                    var jobj = JsonConvert.DeserializeObject<SearchResponse>(txt);
                    if (jobj.tracks?.items?.Any() == true)
                    {
                        var track = jobj.tracks.items[0];
                        return new MusicModel
                        {
                            MusicArtist = string.Join(",", track.artists.Select(p => p.name)),
                            MusicId = track.uri,
                            MusicName = track.name
                        };
                    }
                }
                else
                {
                    AddLog(await result.Content.ReadAsStringAsync());
                    return null;
                }
            }
            else
            {
                return null;
            }

            return null;
        }

        public async Task<bool> WaitLogin()
        {
            if (!listener.Prefixes.Any()) listener.Prefixes.Add("http://localhost:58263/");

            if (listener.IsListening) listener.Stop();
            listener.Start();
            var random = new Random();
            var randombyte = new byte[32];
            random.NextBytes(randombyte);
            var code_veri = Utils.Base64UrlEncode(randombyte);
            var code_cha = Utils.Base64UrlEncode(SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(code_veri)))
                .Replace("=", "");
            var loginargs = new Dictionary<string, string>();
            loginargs["response_type"] = "code";
            loginargs["redirect_uri"] = BLiveSpotify_Plugin.CALLBACK;
            loginargs["client_id"] = BLiveSpotify_Plugin.APPID;
            var scope = new[] { "user-read-playback-state", "user-modify-playback-state", "user-read-private" };
            loginargs["scope"] = string.Join(" ", scope);
            loginargs["code_challenge"] = code_cha;
            loginargs["code_challenge_method"] = "S256";
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var loginarg in loginargs) queryString[loginarg.Key] = loginarg.Value;
            Process.Start("https://accounts.spotify.com/authorize?" + queryString);
            ;
            HttpListenerResponse response = null;
            try
            {
                var context = await listener.GetContextAsync();
                response = context.Response;

                var ret = HttpUtility.ParseQueryString(context.Request.Url.Query);
                if (!string.IsNullOrEmpty(ret["code"]))
                {
                    var code = ret["code"];
                    var nv = HttpUtility.ParseQueryString(string.Empty);
                    nv["grant_type"] = "authorization_code";
                    nv["code"] = code;
                    nv["redirect_uri"] = BLiveSpotify_Plugin.CALLBACK;
                    nv["client_id"] = BLiveSpotify_Plugin.APPID;
                    nv["code_verifier"] = code_veri;
                    var result = await client.PostAsync("https://accounts.spotify.com/api/token",
                        new StringContent(nv.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded"));
                    if (result.IsSuccessStatusCode)
                    {
                        var res = await result.Content.ReadAsStringAsync();
                        var jobj = JObject.Parse(res);
                        refresh_token = jobj["refresh_token"] + "";
                        access_token = jobj["access_token"] + "";
                        expriedate = DateTime.Now.AddSeconds(jobj.Value<int>("expires_in"));
                        SaveConfig();
                        var responseString = "<HTML><META charset=\"UTF-8\"><BODY>授權成功, 請關閉本頁面</BODY></HTML>";
                        var buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        response.ContentType = "text/html";
                        var output = response.OutputStream;
                        await output.WriteAsync(buffer, 0, buffer.Length);
                        await output.FlushAsync();
                        output.Close();
                        return true;
                    }
                    else
                    {
                        var responseString = "<HTML><META charset=\"UTF-8\"><BODY>授權失敗, 請關閉本頁面</BODY></HTML>";
                        var buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        response.ContentType = "text/html";
                        var output = response.OutputStream;
                        await output.WriteAsync(buffer, 0, buffer.Length);
                        await output.FlushAsync();
                        output.Close();
                        return false;
                    }
                }

                {
                    var responseString = "<HTML><META charset=\"UTF-8\"><BODY>授權失敗, 請關閉本頁面</BODY></HTML>";
                    var buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    response.ContentType = "text/html;charset=utf-8";
                    var output = response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    await output.FlushAsync();
                    output.Close();
                }
                return false;
            }
            catch (Exception e)
            {
                if (response != null)
                {
                    var responseString = "<HTML><META charset=\"UTF-8\"><BODY>授權失敗, 請關閉本頁面</BODY></HTML>";
                    var buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    response.ContentType = "text/html;charset=utf-8";
                    var output = response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    await output.FlushAsync();
                    output.Close();
                }

                return false;
            }
            finally
            {
                listener.Stop();
            }
        }

        public static SpotifyLib LoadConfig()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "弹幕姬", "Plugins", "BLiveSpotify_Plugin.token");
            try
            {
                var txt = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<SpotifyLib>(txt);
            }
            catch (Exception e)
            {
                return new SpotifyLib();
            }
        }

        public async Task<bool> AddTrack(string id)
        {
            if (!string.IsNullOrEmpty(playdevice) && await update_token())
            {
                var request = new HttpRequestMessage(HttpMethod.Post,
                    $"https://api.spotify.com/v1/me/player/queue?device_id={HttpUtility.UrlEncode(playdevice)}&uri={HttpUtility.UrlEncode(id)}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }

                AddLog(await result.Content.ReadAsStringAsync());
                return false;
            }

            return false;

            return false;
        }

        public async Task<bool> NextTrack()
        {
            if (!string.IsNullOrEmpty(playdevice) && await update_token())
            {
                var request = new HttpRequestMessage(HttpMethod.Post,
                    $"https://api.spotify.com/v1/me/player/next?device_id={HttpUtility.UrlEncode(playdevice)}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    return true;
                }

                AddLog(await result.Content.ReadAsStringAsync());
                return false;
            }

            return false;

            return false;
        }

        public void SaveConfig()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "弹幕姬", "Plugins", "BLiveSpotify_Plugin.token");
            try
            {
                var txt = JsonConvert.SerializeObject(this);
                File.WriteAllText(path, txt);
            }
            catch (Exception e)
            {
            }
        }

        public void AddLog(string log)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "弹幕姬", "Plugins", "BLiveSpotify_Plugin.log");
            try
            {
                File.AppendAllText(path, log + "\r\n");
            }
            catch (Exception e)
            {
            }
        }
    }

    public class BLiveSpotify_Plugin : DMPlugin
    {
        public const string APPID = "236108c72daa489e98ff9dd537b85109";
        public const string CALLBACK = "http://localhost:58263/callback";
        private MainWindow mp;
        internal SpotifyLib spotifyLib;

        public BLiveSpotify_Plugin()
        {
            spotifyLib = SpotifyLib.LoadConfig();

            ReceivedDanmaku += OnReceivedDanmaku;
            PluginAuth = "CopyLiu";
            PluginName = "Spotify點歌姬";
            PluginCont = "copyliu@gmail.com";
            PluginVer = "v0.0.1";
            PluginDesc = "驚了還有這個";
        }

        public override void Admin()
        {
            base.Admin();
            mp?.Close();
            mp = new MainWindow();
            mp.context.Plugin = this;
            mp.Closed += (sender, args) => mp = null;
            mp.Show();
        }

        private async void OnReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            // e.Danmaku.RawDataJToken;
            if (Status && e.Danmaku.MsgType == MsgTypeEnum.Comment)
            {
                if (e.Danmaku.CommentText?.StartsWith("点歌 ") == true)
                {
                    var searchtxt = e.Danmaku.CommentText.Remove(0, 3);
                    if (!string.IsNullOrEmpty(searchtxt))
                    {
                        var music = await spotifyLib.SearchMusic(searchtxt);
                        if (music != null)
                        {
                            if (!string.IsNullOrEmpty(spotifyLib.playdevice))
                            {
                                var ret = await spotifyLib.AddTrack(music.MusicId);
                                ;
                                if (ret)
                                {
                                    AddDM("新增播放隊列:" + music.MusicName + " - " + music.MusicArtist);
                                    Log("新增播放隊列:" + music.MusicName + " - " + music.MusicArtist);
                                }
                                else
                                {
                                    AddDM("新增播放隊列失敗");
                                    Log("新增播放隊列失敗");
                                }
                            }
                        }
                        else
                        {
                            AddDM("找不到:" + searchtxt);
                            Log("找不到:" + searchtxt);
                        }
                    }
                }

                if (e.Danmaku.CommentText?.StartsWith("track:") == true)
                {
                    var ret = await spotifyLib.AddTrack("spotify:" + e.Danmaku.CommentText);
                    ;
                    if (ret)
                    {
                        AddDM("新增播放隊列:" + "spotify:" + e.Danmaku.CommentText);
                        Log("新增播放隊列:" + "spotify:" + e.Danmaku.CommentText);
                    }
                    else
                    {
                        AddDM("新增播放隊列失敗");
                        Log("新增播放隊列失敗");
                    }
                }

                if (e.Danmaku.CommentText == "切歌" && e.Danmaku.isAdmin)
                {
                    var ret = await spotifyLib.NextTrack();
                    ;
                    if (ret)
                    {
                        AddDM("切歌成功");
                        Log("切歌成功");
                    }
                    else
                    {
                        AddDM("切歌失敗");
                        Log("切歌失敗");
                    }
                }
            }
        }
    }
}