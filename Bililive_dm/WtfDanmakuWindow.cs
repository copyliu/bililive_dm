using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static WINAPI.USER32;

namespace Bililive_dm
{
    using static ExtendedWindowStyles;

    public partial class WtfDanmakuWindow : Form, IDanmakuWindow
    {
        private IntPtr _wtf;

        public WtfDanmakuWindow()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.Manual;
            Resize += WtfDanmakuWindow_Resize;
            FormClosing += WtfDanmakuWindow_FormClosing;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOREDIRECTIONBITMAP;
                return cp;
            }
        }

        void IDisposable.Dispose()
        {
            if (_wtf != IntPtr.Zero)
                DestroyWTF();
        }

        void IDanmakuWindow.Show()
        {
            this.Show();
            WTF_Start(_wtf);
        }

        void IDanmakuWindow.Close()
        {
            this.Close();
        }

        void IDanmakuWindow.ForceTopmost()
        {
            //this.TopMost = false;
            //this.TopMost = true;
        }

        void IDanmakuWindow.AddDanmaku(DanmakuType type, string comment, uint color)
        {
            WTF_AddLiveDanmaku(_wtf, (int)type, 0, comment, 25, (int)color, 0, 0);
        }

        public void SetMonitor(string deviceName)
        {
            var s = Screen.AllScreens.FirstOrDefault(p => p.DeviceName == deviceName) ?? Screen.PrimaryScreen;
            var r = s.WorkingArea;
            WindowState = FormWindowState.Normal;
            Top = r.Top;
            Left = r.Left;
            Width = r.Width;
            Height = r.Height;
            WindowState = FormWindowState.Maximized;
        }

        void IDanmakuWindow.OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_wtf != IntPtr.Zero)
            {
                WTF_SetFontScaleFactor(_wtf, (float)(Store.FullOverlayFontsize / 25.0f));
                SetWindowDisplayAffinity(_wtf, Store.DisplayAffinity ? WindowDisplayAffinity.ExcludeFromCapture : 0);
                if (e.PropertyName == nameof(Store.FullScreenMonitor)) SetMonitor(Store.FullScreenMonitor);
            }
        }

        [DllImport("libwtfdanmaku")]
        private static extern IntPtr WTF_CreateInstance();

        [DllImport("libwtfdanmaku")]
        private static extern void WTF_ReleaseInstance(IntPtr instance);

        [DllImport("libwtfdanmaku")]
        private static extern int WTF_InitializeWithHwnd(IntPtr instance, IntPtr hwnd);

        [DllImport("libwtfdanmaku")]
        private static extern int WTF_InitializeOffscreen(IntPtr instance, uint initialWidth, uint initialHeight);

        [DllImport("libwtfdanmaku")]
        private static extern void WTF_Terminate(IntPtr instance);

        [DllImport("libwtfdanmaku", CharSet = CharSet.Unicode)]
        private static extern void WTF_AddLiveDanmaku(IntPtr instance, int type, long time, string comment,
            int fontSize, int fontColor, long timestamp, int danmakuId);

        [DllImport("libwtfdanmaku")]
        private static extern void WTF_Start(IntPtr instance);

        [DllImport("libwtfdanmaku")]
        private static extern void WTF_Pause(IntPtr instance);

        [DllImport("libwtfdanmaku")]
        private static extern void WTF_Resume(IntPtr instance);

        [DllImport("libwtfdanmaku")]
        private static extern void WTF_Stop(IntPtr instance);

        [DllImport("libwtfdanmaku")]
        private static extern void WTF_Resize(IntPtr instance, uint width, uint height);

        [DllImport("libwtfdanmaku")]
        private static extern int WTF_IsRunning(IntPtr instance);

        [DllImport("libwtfdanmaku")]
        private static extern float WTF_GetFontScaleFactor(IntPtr instance);

        [DllImport("libwtfdanmaku")]
        private static extern void WTF_SetFontScaleFactor(IntPtr instance, float factor);

        [DllImport("libwtfdanmaku", CharSet = CharSet.Unicode)]
        private static extern void WTF_SetFontName(IntPtr instance, string fontName);

        [DllImport("libwtfdanmaku")]
        private static extern void WTF_SetDanmakuStyle(IntPtr instance, int style);

        [DllImport("libwtfdanmaku")]
        private static extern void WTF_SetCompositionOpacity(IntPtr instance, float opacity);

        ~WtfDanmakuWindow()
        {
            (this as IDanmakuWindow).Dispose();
        }

        private void WtfDanmakuWindow_Load(object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            var exStyles = GetExtendedWindowStyles(Handle);
            SetExtendedWindowStyles(Handle, exStyles | Layered | Transparent | ToolWindow);

            CreateWTF();
        }

        private void WtfDanmakuWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            DestroyWTF();
        }

        private void WtfDanmakuWindow_Resize(object sender, EventArgs e)
        {
            if (_wtf != IntPtr.Zero) WTF_Resize(_wtf, (uint)ClientSize.Width, (uint)ClientSize.Height);
        }

        private void CreateWTF()
        {
            _wtf = WTF_CreateInstance();
            WTF_InitializeWithHwnd(_wtf, Handle);
            WTF_SetFontName(_wtf, "SimHei");
            WTF_SetFontScaleFactor(_wtf, (float)(Store.FullOverlayFontsize / 25.0f));
            WTF_SetCompositionOpacity(_wtf, 0.85f);
            SetWindowDisplayAffinity(_wtf, Store.DisplayAffinity ? WindowDisplayAffinity.ExcludeFromCapture : 0);
            SetMonitor(Store.FullScreenMonitor);
        }

        private void DestroyWTF()
        {
            if (_wtf != IntPtr.Zero)
            {
                if (WTF_IsRunning(_wtf) != 0) WTF_Stop(_wtf);
                WTF_Terminate(_wtf);
                WTF_ReleaseInstance(_wtf);
                _wtf = IntPtr.Zero;
            }
        }
    }
}