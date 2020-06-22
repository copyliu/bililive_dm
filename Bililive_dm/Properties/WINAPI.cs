using System;
using System.Runtime.InteropServices;

static partial class WINAPI
{
    const string KERNEL32 = nameof(KERNEL32);

    [DllImport(KERNEL32)]
    internal static extern void SetLastError(int code);

    [DllImport(KERNEL32)]
    public static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr min, IntPtr max);
    public static bool SetProcessWorkingSetSize(IntPtr hProcess, int min, int max) => SetProcessWorkingSetSize(hProcess, (IntPtr)min, (IntPtr)max);
    public static bool ReleasePages(IntPtr hProcess) => SetProcessWorkingSetSize(hProcess, -1, -1);

    [DllImport(KERNEL32, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool SetDllDirectory(string path);
}
