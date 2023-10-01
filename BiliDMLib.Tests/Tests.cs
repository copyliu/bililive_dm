using System;
using BilibiliDM_PluginFramework;
using Xunit;

namespace BiliDMLib.Tests
{
    public class Tests
    {
        [Fact]
        public void GuardTest()
        {
            var guardjson =
                "{\n  \"cmd\": \"LIVE_OPEN_PLATFORM_GUARD\",\n  \"data\": {\n    \"user_info\": {\n      \"uid\": 110000331,\n      \"uname\": \"\",\n      \"uface\": \"http://i0.hdslb.com/bfs/face/4add3acfc930fcd07d06ea5e10a3a377314141c2.jpg\"\n    },\n    \"guard_level\": 3,\n    \"guard_num\": 1,\n    \"guard_unit\": \"月\",\n    \"fans_medal_level\": 24,\n    \"fans_medal_name\": \"aw4ifC\",\n    \"fans_medal_wearing_status\": false,\n    \"timestamp\": 1653555128,\n    \"room_id\": 460695,\n    \"msg_id\": \"\"\n  }\n}";
            var model = new DanmakuModel(guardjson,2);
            Assert.Equal(MsgTypeEnum.GuardBuy, model.MsgType);
        }
    }
}