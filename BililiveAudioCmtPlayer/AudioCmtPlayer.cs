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
        private MainPage mp;
        private PluginDataContext context = new PluginDataContext();

        private HttpClient httpClient = new HttpClient(new HttpClientHandler()
            {AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip})
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public AudioCmtPlayer()
        {
            this.ReceivedDanmaku += OnReceivedDanmaku;
            this.PluginAuth = "CopyLiu";
            this.PluginName = "語音彈幕播放插件";
            this.PluginCont = "copyliu@gmail.com";
            this.PluginVer = "v0.0.1";
            this.PluginDesc = "(測試版) 啟用後會自動播放語音彈幕, 管理界面查看播放列表";
            context.Plugin = this;
            var _ = AudioPlay(CancellationToken.None);

        }

        private void OnReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            if (e.Danmaku.MsgType == MsgTypeEnum.Comment)
            {
                var audioobj = e.Danmaku.RawDataJToken["info"][0][14];
                if (audioobj != null && audioobj["voice_url"] != null)
                {
                    Application.Current.Dispatcher
                        .BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            Log($"语音弹幕: {e.Danmaku.UserName}: {e.Danmaku.CommentText}");
                            context.DataList.Add(new DMItem()
                            {
                                ItemName = $"{DateTime.Now:HH:mm:ss} {e.Danmaku.UserName}: {e.Danmaku.CommentText}",
                                Model = e.Danmaku
                            });
                        }));
                }

            }
        }

        public override void Admin()
        {
            base.Admin();
            this.mp = new MainPage(context);
            this.mp.Closed += (sender, args) => this.mp = null;
            this.mp.Show();
        }



        async Task AudioPlay(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {

                {
                    var model = context.DataList.FirstOrDefault();
                    if (model != null)
                    {
                        try
                        {
                            var audioobj = model.Model.RawDataJToken["info"][0][14];
                            if (audioobj != null && audioobj["voice_url"] != null)
                            {
                               
                                {
                                    var cts = new CancellationTokenSource();
                                    var urlstring = audioobj["voice_url"] + "";
                                    var url = new Uri(WebUtility.UrlDecode(urlstring));
                                    var streamtask = httpClient.GetStreamAsync(url);
                                    streamtask.Wait(CancellationToken.None);
                                    var stream = streamtask.Result;

                                    var filename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() +
                                                   System.IO.Path.GetExtension(url.LocalPath);
                                    var mediaPlayer = new MediaPlayer();
                                    mediaPlayer.Volume = 1;
                                    mediaPlayer.MediaEnded += (sender, args) => cts.Cancel();
                                    mediaPlayer.MediaFailed += (sender, args) =>
                                    {

                                        this.Log(args.ErrorException.ToString());
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

                        }
                        catch (Exception e)
                        {
                            this.Log(e.ToString());
                            // ignored
                        }
                        finally
                        {

                            context.DataList.Remove(model);
                        }

                    }



                    await Task.Delay(TimeSpan.FromSeconds(0.5), token);
                }
            }
        }
    }
}