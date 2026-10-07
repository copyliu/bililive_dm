using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bililive_dm
{
    static class WineDetector
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]

        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]

        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);


        public static bool IsRunningUnderWine()
        {
            IntPtr hNtdll = GetModuleHandle("ntdll.dll");
            if (hNtdll == IntPtr.Zero)
            {
              
            }
            else
            {
                // Check for other unique wine exports instead of wine_get_version
                var processwine = GetProcAddress(hNtdll, "wine_get_host_version") != IntPtr.Zero ||
                       GetProcAddress(hNtdll, "wine_server_call") != IntPtr.Zero;
                if (processwine)
                {
                    return true;
                }
            }

          

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Wine"))
            {
                if (key != null)
                {
                    return true;
                }
            }
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wine"))
            {
                if (key != null)
                {
                    return true;
                }
            }
            // Native Windows always has winlogon running for the active user session
            bool hasWinlogon = Process.GetProcessesByName("winlogon").Any();

            // Wine environments often expose winedevice
            bool hasWineDevice = Process.GetProcessesByName("winedevice").Any();
            
            return !hasWinlogon || hasWineDevice;
        }
    }
}
