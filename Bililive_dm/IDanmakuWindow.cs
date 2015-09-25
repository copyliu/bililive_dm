using System;
using System.Collections.Generic;
using System.Text;

namespace Bililive_dm
{
    public enum DanmakuType
    {
        Scrolling = 1,
        Bottom = 4,
        Top = 5,
        Reserve = 6
    }

    public interface IDanmakuWindow
    {
        void Initialize();
        void Terminate();
        void Show();
        void Close();
        void AddDanmaku(DanmakuType type, string comment, uint color);
    }
}
