using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

partial class WINAPI
{
    internal static class USER32
    {
        const string MODULENAME = nameof(USER32);

        #region Window Style

        /* When and only when accessing Window Styles & Extended Window Styles
         *  you can and you should use the non-pointer variants of Window-Long
         *  functions. */

        public const int WS_EX_TRANSPARENT = 0x20;
        public const int WS_EX_TOOLWINDOW = 0x00000080; // 不在Alt-Tab中显示 && Win10下，在所有虚拟桌面显示
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

        public enum ExtendedWindowStyles
        {
            // ...
            Transparent = WS_EX_TRANSPARENT,
            ToolWindow = WS_EX_TOOLWINDOW, // 不在Alt-Tab中显示 && Win10下，在所有虚拟桌面显示
            Layered = WS_EX_LAYERED,
            Noredirectionbitmap = WS_EX_NOREDIRECTIONBITMAP,
            // ...
        }

        public enum WindowStylesKind
        {
            Styles = -16,
            ExStyles = -20,
        }

        [DllImport(MODULENAME)]
        public static extern int GetWindowLong(IntPtr hWnd, WindowStylesKind kind);
        public static ExtendedWindowStyles GetExtendedWindowStyles(IntPtr hWnd)
            => (ExtendedWindowStyles)GetWindowLong(hWnd, WindowStylesKind.ExStyles);

        [DllImport(MODULENAME, SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, WindowStylesKind kind, int styles);
        public static ExtendedWindowStyles SetExtendedWindowStyles(IntPtr hWnd, ExtendedWindowStyles styles)
            => (ExtendedWindowStyles)SetWindowLong(hWnd, WindowStylesKind.ExStyles, (int)styles);

        #endregion

        public enum WindowDisplayAffinity
        {
            None,
            Monitor,
            ExcludeFromCapture = 0x10 | Monitor,
        }
        [DllImport(MODULENAME)]
        public static extern bool SetWindowDisplayAffinity(IntPtr hWnd, WindowDisplayAffinity dwAffinity);
    }
}
