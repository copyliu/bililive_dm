using System;
using System.Collections.Generic;
using System.Linq;

namespace BilibiliDM_PluginFramework
{
    public delegate void DisconnectEvt(object sender, DisconnectEvtArgs e);

    public delegate void ReceivedDanmakuEvt(object sender, ReceivedDanmakuArgs e);

    public delegate void ReceivedRoomCountEvt(object sender, ReceivedRoomCountArgs e);
    public delegate void ConnectedEvt(object sender, ConnectedEvtArgs e);

    public class ReceivedRoomCountArgs
    {
        public uint UserCount;
    }

    public class DisconnectEvtArgs
    {
        public Exception Error;
    }

    public class ReceivedDanmakuArgs
    {
        public DanmakuModel Danmaku;
    }
    public class ConnectedEvtArgs
    {
        public int roomid;
    }
}