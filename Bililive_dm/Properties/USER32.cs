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

        public enum WindowStylesKind
        {
            Styles = -16,
            ExStyles = -20,
        }

        [DllImport(MODULENAME, EntryPoint = "GetWindowLong", SetLastError = true)]
        public static extern int GetWindowStyles(IntPtr hWnd, WindowStylesKind kind);
        public static ExtendedWindowStyles GetExtendedWindowStyles(IntPtr hWnd)
            => (ExtendedWindowStyles)GetWindowStyles(hWnd, WindowStylesKind.ExStyles);

        [DllImport(MODULENAME, EntryPoint = "SetWindowLong", SetLastError = true)]
        public static extern int SetWindowStyles(IntPtr hWnd, WindowStylesKind kind, int styles);
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
