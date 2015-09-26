using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Bililive_dm
{
    public enum DanmakuType
    {
        Scrolling = 1,
        Bottom = 4,
        Top = 5,
        Reserve = 6
    }

    public interface IDanmakuWindow : IDisposable
    {
        void Show();
        void Close();
        void ForceTopmost();
        void OnPropertyChanged(object sender, PropertyChangedEventArgs e);
        void AddDanmaku(DanmakuType type, string comment, uint color);
    }
}
