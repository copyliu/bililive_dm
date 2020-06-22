using System;
using System.Runtime.InteropServices;

partial class WINAPI
{
    internal static class USER32
    {
        const string MODULENAME = nameof(USER32);

        #region Window Style

        /* When and only when accessing Window Styles & Extended Window Styles
         *  you can and you should use the non-pointer variants of Window-Long
         *  functions. */

        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_TOOLWINDOW = 0x00000080; // 不在Alt-Tab中显示 && Win10下，在所有虚拟桌面显示
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

        public enum ExtendedWindowStyles
        {
            // ...
            Transparent = WS_EX_TRANSPARENT,
            ToolWindow = WS_EX_TOOLWINDOW, // 不在Alt-Tab中显示 && Win10下，在所有虚拟桌面显示
            Layered = WS_EX_LAYERED,
            NoRedirectionBitmap = WS_EX_NOREDIRECTIONBITMAP, // fat chance
            // ...
        }

        [Obsolete]
        public const int GWL_EXSTYLE = -20;
        [Obsolete]
        public const int GWL_HINSTANCE = GWLP_HINSTANCE;
        const int GWLP_HINSTANCE = -6;
        [Obsolete]
        public const int GWL_ID = GWLP_ID;
        const int GWLP_ID = -12;
        [Obsolete]
        public const int GWL_STYLE = -16;
        [Obsolete]
        public const int GWL_USERDATA = GWLP_USERDATA;
        const int GWLP_USERDATA = -21;
        [Obsolete]
        public const int GWL_WNDPROC = GWLP_WNDPROC;
        const int GWLP_WNDPROC = -4;
        [Obsolete]
        public const int DWL_MSGRESULT = DWLP_MSGRESULT;
        const int DWLP_MSGRESULT = 0;

        public enum WindowStylesKind
        {
#pragma warning disable CS0612 // 类型或成员已过时
            Styles = GWL_STYLE,
            ExStyles = GWL_EXSTYLE,
#pragma warning restore CS0612 // 类型或成员已过时
        }

        [Obsolete, DllImport(MODULENAME, SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
#pragma warning disable CS0612 // 类型或成员已过时
        public static int GetWindowStyles(IntPtr hWnd, WindowStylesKind kind) => GetWindowLong(hWnd, (int)kind);
#pragma warning restore CS0612 // 类型或成员已过时
        public static ExtendedWindowStyles GetExtendedWindowStyles(IntPtr hWnd)
            => (ExtendedWindowStyles)GetWindowStyles(hWnd, WindowStylesKind.ExStyles);

        [Obsolete, DllImport(MODULENAME, SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
#pragma warning disable CS0612 // 类型或成员已过时
        public static int SetWindowStyles(IntPtr hWnd, WindowStylesKind kind, int styles) => SetWindowLong(hWnd, (int)kind, styles);
#pragma warning restore CS0612 // 类型或成员已过时
        public static ExtendedWindowStyles SetExtendedWindowStyles(IntPtr hWnd, ExtendedWindowStyles styles)
            => (ExtendedWindowStyles)SetWindowStyles(hWnd, WindowStylesKind.ExStyles, (int)styles);

        #endregion

        public enum WindowDisplayAffinity
        {
            None,
            Monitor,
            ExcludeFromCapture = 0x10 | Monitor,
        }
        [DllImport(MODULENAME, SetLastError = true)]
        public static extern bool SetWindowDisplayAffinity(IntPtr hWnd, WindowDisplayAffinity affinity);
    }
}
