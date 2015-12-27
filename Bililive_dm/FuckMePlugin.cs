using System.Windows.Forms;
using BilibiliDM_PluginFramework;
using BiliDMLib;

namespace Bililive_dm
{
    public sealed class FuckMePlugin:DMPlugin
    {
        private DanmakuLoader b = new BiliDMLib.DanmakuLoader();
        public FuckMePlugin()
        {
            this.PluginDesc = "这是流氓插件, 用来让作者大人可以刷你们屏. 副作用是5051会多一个观众, 不过无所谓了";
            this.PluginAuth = "CopyLiu";
            this.PluginCont = "copyliu@gmail.com";
            this.PluginName = "作者要耍流氓";
            this.PluginVer = "⑨";
            this.Start();
            b.Disconnected += B_Disconnected;
            b.ReceivedDanmaku += B_ReceivedDanmaku;
        }

        public override async void Start()
        {
            base.Start();
            
            
            var result = await b.ConnectAsync(5051);

           
        }

        private async void B_Disconnected(object sender, DisconnectEvtArgs e)
        {
            await b.ConnectAsync(5051);
        }

        private void B_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            if (e.Danmaku.MsgType == MsgTypeEnum.LiveStart)
            {
                AddDM("黑猫老爷的直播间5051打开了!");
            }
        }

        public override void Stop()
        {
            MessageBox.Show("流氓插件不允许禁用", "报警了!");
        }
    }
}