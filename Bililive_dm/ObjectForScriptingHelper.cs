using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Bililive_dm
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class ObjectForScriptingHelper
    {
        private MainWindow mExternalWPF;

        public ObjectForScriptingHelper(MainWindow w)
        {
            mExternalWPF = w;
        }

        public void OpenGitHub()
        {
            Process.Start("https://github.com/copyliu/bililive_dm");
            ;
        }

        public void OpenUWPStore()
        {
            Process.Start("https://apps.microsoft.com/detail/9PBVHQH1P2BV");
            ;
        }
    }
}