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

        [Flags()]
        public enum SetWindowPosFlags : uint
        {
            /// <summary>If the calling thread and the thread that owns the window are attached to different input queues,
            /// the system posts the request to the thread that owns the window. This prevents the calling thread from
            /// blocking its execution while other threads process the request.</summary>
            /// <remarks>SWP_ASYNCWINDOWPOS</remarks>
            AsynchronousWindowPosition = 0x4000,
            /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
            /// <remarks>SWP_DEFERERASE</remarks>
            DeferErase = 0x2000,
            /// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
            /// <remarks>SWP_DRAWFRAME</remarks>
            DrawFrame = 0x0020,
            /// <summary>Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to
            /// the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE
            /// is sent only when the window's size is being changed.</summary>
            /// <remarks>SWP_FRAMECHANGED</remarks>
            FrameChanged = 0x0020,
            /// <summary>Hides the window.</summary>
            /// <remarks>SWP_HIDEWINDOW</remarks>
            HideWindow = 0x0080,
            /// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the
            /// top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
            /// parameter).</summary>
            /// <remarks>SWP_NOACTIVATE</remarks>
            DoNotActivate = 0x0010,
            /// <summary>Discards the entire contents of the client area. If this flag is not specified, the valid
            /// contents of the client area are saved and copied back into the client area after the window is sized or
            /// repositioned.</summary>
            /// <remarks>SWP_NOCOPYBITS</remarks>
            DoNotCopyBits = 0x0100,
            /// <summary>Retains the current position (ignores X and Y parameters).</summary>
            /// <remarks>SWP_NOMOVE</remarks>
            IgnoreMove = 0x0002,
            /// <summary>Does not change the owner window's position in the Z order.</summary>
            /// <remarks>SWP_NOOWNERZORDER</remarks>
            DoNotChangeOwnerZOrder = 0x0200,
            /// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to
            /// the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent
            /// window uncovered as a result of the window being moved. When this flag is set, the application must
            /// explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</summary>
            /// <remarks>SWP_NOREDRAW</remarks>
            DoNotRedraw = 0x0008,
            /// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
            /// <remarks>SWP_NOREPOSITION</remarks>
            DoNotReposition = 0x0200,
            /// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
            /// <remarks>SWP_NOSENDCHANGING</remarks>
            DoNotSendChangingEvent = 0x0400,
            /// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
            /// <remarks>SWP_NOSIZE</remarks>
            IgnoreResize = 0x0001,
            /// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
            /// <remarks>SWP_NOZORDER</remarks>
            IgnoreZOrder = 0x0004,
            /// <summary>Displays the window.</summary>
            /// <remarks>SWP_SHOWWINDOW</remarks>
            ShowWindow = 0x0040,
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

        [DllImport(MODULENAME, SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
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
