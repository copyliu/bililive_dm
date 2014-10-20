using System;
using System.Collections.Generic;
using System.Linq;

namespace BiliDMLib
{
    public delegate void DisconnectEvt(object sender, DisconnectEvtArgs args);

    public delegate void ReceivedDanmakuEvt(object sender, ReceivedDanmakuArgs e);

    public delegate void ReceivedRoomCountEvt(object sender, ReceivedRoomCountArgs e);

    public class ReceivedRoomCountArgs
    {
        public int UserCount;
    }

    public class DisconnectEvtArgs
    {
        public Exception Error;
    }

    public class ReceivedDanmakuArgs
    {
        public DanmakuModel Danmaku;
    }
}