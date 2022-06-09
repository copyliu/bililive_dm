using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32;

namespace Bililive_dm
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class ObjectForScriptingHelper
    {
        MainWindow mExternalWPF;
        public ObjectForScriptingHelper(MainWindow w)
        {
            this.mExternalWPF = w;
        }
        public bool Get472FromRegistry()
        {
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                var releaseKey = Convert.ToInt32(ndpKey?.GetValue("Release"));
                return releaseKey >= 461808;
            }
        }
        public void OpenGitHub()
        {
            System.Diagnostics.Process.Start("https://github.com/copyliu/bililive_dm"); ;
        }
        public void OpenUWPStore()
        {
            System.Diagnostics.Process.Start("https://www.microsoft.com/store/apps/9PBVHQH1P2BV"); ;
        }
        public void OpenNETDownload()
        {
            System.Diagnostics.Process.Start("https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/thank-you/net48-web-installer"); ;
        }
    }
}