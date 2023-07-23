using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using BilibiliDM_PluginFramework;

namespace BililiveAudioCmtPlayer
{
    public class AudioCmtPlayer : DMPlugin
    {
        private readonly PluginDataContext context = new PluginDataContext();

        private readonly HttpClient httpClient = new HttpClient(new HttpClientHandler
            { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private MainPage mp;

        public AudioCmtPlayer()
        {
            ReceivedDanmaku += OnReceivedDanmaku;
            PluginAuth = "CopyLiu";
            PluginName = "語音彈幕播放插件";
            PluginCont = "copyliu@gmail.com";
            PluginVer = "v0.0.1";
            PluginDesc = "(測試版) 啟用後會自動播放語音彈幕, 管理界面查看播放列表";
            context.Plugin = this;
            var _ = AudioPlay(CancellationToken.None);
        }

        private void OnReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            if (e.Danmaku.MsgType == MsgTypeEnum.Comment)
            {
                var audioobj = e.Danmaku.RawDataJToken["info"][0][14];
                if (audioobj != null && audioobj["voice_url"] != null)
                    Application.Current.Dispatcher
                        .BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            Log($"语音弹幕: {e.Danmaku.UserName}: {e.Danmaku.CommentText}");
                            context.DataList.Add(new DMItem
                            {
                                ItemName = $"{DateTime.Now:HH:mm:ss} {e.Danmaku.UserName}: {e.Danmaku.CommentText}",
                                Model = e.Danmaku
                            });
                        }));
            }
        }

        public override void Admin()
        {
            base.Admin();
            mp = new MainPage(context);
            mp.Closed += (sender, args) => mp = null;
            mp.Show();
        }


        private async Task AudioPlay(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var model = context.DataList.FirstOrDefault();
                if (model != null)
                    try
                    {
                        var audioobj = model.Model.RawDataJToken["info"][0][14];
                        if (audioobj != null && audioobj["voice_url"] != null)
                        {
                            var cts = new CancellationTokenSource();
                            var urlstring = audioobj["voice_url"] + "";
                            var url = new Uri(WebUtility.UrlDecode(urlstring));
                            var streamtask = httpClient.GetStreamAsync(url);
                            streamtask.Wait(CancellationToken.None);
                            var stream = streamtask.Result;

                            var filename = Path.GetTempPath() + Guid.NewGuid() +
                                           Path.GetExtension(url.LocalPath);
                            var mediaPlayer = new MediaPlayer();
                            mediaPlayer.Volume = 1;
                            mediaPlayer.MediaEnded += (sender, args) => cts.Cancel();
                            mediaPlayer.MediaFailed += (sender, args) =>
                            {
                                Log(args.ErrorException.ToString());
                                cts.Cancel();
                            };
                            try
                            {
                                using (var fileStream = File.Create(filename))
                                {
                                    await stream.CopyToAsync(fileStream);
                                }


                                mediaPlayer.Open(new Uri(filename));
                                mediaPlayer.Play();
                                try
                                {
                                    await Task.Delay(-1, cts.Token);
                                }
                                catch (Exception e)
                                {
                                    //ignore
                                }
                            }

                            finally
                            {
                                mediaPlayer.Close();
                                File.Delete(filename);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log(e.ToString());
                        // ignored
                    }
                    finally
                    {
                        context.DataList.Remove(model);
                    }


                await Task.Delay(TimeSpan.FromSeconds(0.5), token);
            }
        }
    }
}