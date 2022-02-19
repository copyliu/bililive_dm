using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
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

        public string refresh_token { get; set; }
        public string playdevice { get; set; }
        private string access_token;
        private DateTime expriedate;
        private HttpClient client = new HttpClient();
        private HttpListener listener = new HttpListener();

        private async Task<bool> update_token()
        {
            if (string.IsNullOrWhiteSpace(refresh_token))
            {
                return false;
            }

            if ((expriedate < DateTime.Now || string.IsNullOrEmpty(access_token)))
            {
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
                        this.access_token = jobj["access_token"] + "";
                        this.expriedate = DateTime.Now.AddSeconds(jobj.Value<int>("expires_in"));
                        this.refresh_token = jobj["refresh_token"] + "";
                        this.SaveConfig();
                        return true;
                    }
                    else
                    {
                        this.refresh_token = "";
                        this.SaveConfig();
                        return false;
                    }
                }
                catch (Exception e)
                {
                    this.refresh_token = "";
                    this.SaveConfig();
                    return false;
                }

            }

            return true;
        }


        public async Task<List<PlayDeviceModel>> GetPlayDevices()
        {
            if (await update_token())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/devices");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.access_token);
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var txt = await result.Content.ReadAsStringAsync();
                    var jobj = JsonConvert.DeserializeObject<PlayDeviceResponse>(txt);
                    var results=jobj.devices.Select(p => new PlayDeviceModel()
                    {
                        PlaylistId = p.id,
                        PlaylistName = p.name
                    }).ToList();
                  

                    return results;
                }
                else
                {
                    throw new MyException("獲取歌單失敗");



                }
            }
            else
            {
                throw new MyException("授權失效");
            }
             

        }

        public async Task<MusicModel> SearchMusic(string name)
        {
            if (await update_token())
            {
           
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/search?type=track&include_external=audio&limit=1&q={ HttpUtility.UrlEncode(name)}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.access_token);
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var txt = await result.Content.ReadAsStringAsync();
                    var jobj = JsonConvert.DeserializeObject<SearchResponse>(txt);
                    if (jobj.tracks?.items?.Any()==true)
                    {
                        var track = jobj.tracks.items[0];
                        return new MusicModel()
                        {
                            MusicArtist = string.Join(",", track.artists.Select(p=>p.name)),
                            MusicId = track.uri,
                            MusicName = track.name
                        };
                        
                    }

                }
                else
                {
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
            if (!listener.Prefixes.Any())
            {listener.Prefixes.Add("http://localhost:58263/");}

            if (listener.IsListening)
            {
                listener.Stop();
            }
            listener.Start();
            var random = new Random();
            var randombyte = new byte[32];
            random.NextBytes(randombyte);
            var code_veri = Utils.Base64UrlEncode(randombyte);
            var code_cha = Utils.Base64UrlEncode(SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(code_veri))).Replace("=", "");
            var loginargs = new Dictionary<string, string>();
            loginargs["response_type"] = "code";
            loginargs["redirect_uri"] = BLiveSpotify_Plugin.CALLBACK;
            loginargs["client_id"] = BLiveSpotify_Plugin.APPID;
            var scope = new[] { "user-read-playback-state", "user-modify-playback-state", "user-read-private" };
            loginargs["scope"] = string.Join(" ", scope);
            loginargs["code_challenge"] = code_cha;
            loginargs["code_challenge_method"] = "S256";
            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            foreach (var loginarg in loginargs)
            {
                queryString[loginarg.Key]= loginarg.Value;
                
            }
            System.Diagnostics.Process.Start("https://accounts.spotify.com/authorize?"+queryString); ;
            try
            {
                var context = await listener.GetContextAsync();
                var response = context.Response;
               
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
                        this.refresh_token = jobj["refresh_token"] + "";
                        this.access_token = jobj["access_token"] + "";
                        this.expriedate = DateTime.Now.AddSeconds(jobj.Value<int>("expires_in"));
                        this.SaveConfig();
                        string responseString = "<HTML><META charset=\"UTF-8\"><BODY>授權成功, 請關閉本頁面</BODY></HTML>";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        response.ContentType = "text/html";
                        System.IO.Stream output = response.OutputStream;
                        await output.WriteAsync(buffer, 0, buffer.Length);
                        output.Close();
                        return true;
                    }
                    else
                    {
                        string responseString = "<HTML><META charset=\"UTF-8\"><BODY>授權失敗, 請關閉本頁面</BODY></HTML>";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        response.ContentType = "text/html";
                        System.IO.Stream output = response.OutputStream;
                        await output.WriteAsync(buffer, 0, buffer.Length);
                        output.Close();
                        return false;
                    }
                }

                {
                    string responseString = "<HTML><META charset=\"UTF-8\"><BODY>授權失敗, 請關閉本頁面</BODY></HTML>";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    response.ContentType = "text/html;charset=utf-8";
                    System.IO.Stream output = response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    output.Close();
                }
                return false;
            }
            catch (Exception e)
            {

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
            if (!string.IsNullOrEmpty(this.playdevice)&&await update_token())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.spotify.com/v1/me/player/queue?device_id={HttpUtility.UrlEncode(playdevice)}&uris={HttpUtility.UrlEncode(id)}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.access_token);
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return false;
        }

        public async Task<bool> NextTrack()
        {
            if (!string.IsNullOrEmpty(this.playdevice) && await update_token())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.spotify.com/v1/me/player/next?device_id={HttpUtility.UrlEncode(playdevice)}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.access_token);
                var result = await client.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return false;

        }

        public  void SaveConfig()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "弹幕姬", "Plugins", "BLiveSpotify_Plugin.token");
            try
            {
                var txt = JsonConvert.SerializeObject(this);
                File.WriteAllText(path,txt);
               
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
            spotifyLib=SpotifyLib.LoadConfig();
            
            this.ReceivedDanmaku += OnReceivedDanmaku;
            this.PluginAuth = "CopyLiu";
            this.PluginName = "Spotify點歌姬";
            this.PluginCont = "copyliu@gmail.com";
            this.PluginVer = "v0.0.1";
            this.PluginDesc = "驚了還有這個";
        }

        public override void Admin()
        {
            base.Admin();
            this.mp?.Close();
            this.mp = new MainWindow();
            this.mp.context.Plugin = this;
            this.mp.Closed += (sender, args) => this.mp = null;
            this.mp.Show();
        }
        private async void OnReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            // e.Danmaku.RawDataJToken;
            if (Status && e.Danmaku.MsgType==MsgTypeEnum.Comment)
            {
                if (e.Danmaku.CommentText?.StartsWith("点歌 ")==true)
                {
                    var searchtxt = e.Danmaku.CommentText.Remove(0, 3);
                    if (!string.IsNullOrEmpty(searchtxt))
                    {
                        var music = await this.spotifyLib.SearchMusic(searchtxt);
                        if (music != null)
                        {
                            if (!string.IsNullOrEmpty(spotifyLib.playdevice))
                            {
                                
                                var ret=await spotifyLib.AddTrack(music.MusicId);
                                ;
                                if (ret)
                                {
                                    this.AddDM("新增播放隊列:" + music.MusicName + " - " + music.MusicArtist,false);
                                    Log("新增播放隊列:" + music.MusicName + " - " + music.MusicArtist);
                                }
                                else
                                {
                                    this.AddDM("新增播放隊列失敗", false);
                                    Log("新增播放隊列失敗");
                                }
                            }
                        }
                        else
                        {
                            this.AddDM("找不到:"+searchtxt, false);
                            Log("找不到:"+searchtxt );
                        }
                    }
                   
                }

                if (e.Danmaku.CommentText?.StartsWith("track:") == true)
                {
                    var ret = await spotifyLib.AddTrack("spotify:"+e.Danmaku.CommentText);
                    ;
                    if (ret)
                    {
                        this.AddDM("新增播放隊列:" + "spotify:" + e.Danmaku.CommentText, false);
                        Log("新增播放隊列:" + "spotify:" + e.Danmaku.CommentText);
                    }
                    else
                    {
                        this.AddDM("新增播放隊列失敗", false);
                        Log("新增播放隊列失敗");
                    }
                }

                if (e.Danmaku.CommentText == "切歌" && e.Danmaku.isAdmin)
                {
                    var ret = await spotifyLib.NextTrack();
                    ;
                    if (ret)
                    {
                        this.AddDM("切歌成功");
                        Log("切歌成功");
                    }
                    else
                    {
                        this.AddDM("切歌失敗", false);
                        Log("切歌失敗");
                    }
                }
            }
        }
    }
}
