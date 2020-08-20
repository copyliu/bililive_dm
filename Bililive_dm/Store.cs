using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using BilibiliDM_PluginFramework;

namespace Bililive_dm
{
    public static class Store
    {
        public static double MainOverlayXoffset = 0;
        public static double MainOverlayYoffset = 0;
        public static double MainOverlayWidth = 250;
        public static double MainOverlayEffect1 = 0.8; //拉伸
        public static double MainOverlayEffect2 = 1.4 - 0.8; //文字出現
        public static double MainOverlayEffect3 = 6 - 1.4; //文字停留
        public static double MainOverlayEffect4 = 1; //窗口消失
        public static double MainOverlayFontsize = 18.667;


        public static double FullOverlayEffect1 = 400; //文字速度
        public static double FullOverlayFontsize = 35;
        public static bool WtfEngineEnabled = true;
        public static bool DisplayAffinity = false;
        public static string FullScreenMonitor = null;
    }


    public static class DefaultStore
    {
        public static double MainOverlayXoffset = 0;
        public static double MainOverlayYoffset = 0;
        public static double MainOverlayWidth = 250;
        public static double MainOverlayEffect1 = 0.8; //拉伸
        public static double MainOverlayEffect2 = 1.4 - 0.8; //文字出現
        public static double MainOverlayEffect3 = 6 - 1.4; //文字停留
        public static double MainOverlayEffect4 = 1; //窗口消失
        public static double MainOverlayFontsize = 18.667;


        public static double FullOverlayEffect1 = 400; //文字速度
        public static double FullOverlayFontsize = 35;
        public static bool WtfEngineEnabled = true;
        public static bool DisplayAffinity = false;
        public static string FullScreenMonitor = null;
    }

    public static class Utils
    {
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAddressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAddressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAddressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAddressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }
        public static void PluginExceptionHandler(Exception ex, DMPlugin plugin=null)
        {
           
                if (plugin != null)
                {
                    MessageBox.Show(
                        "插件" + plugin.PluginName + "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給該插件作者 " + plugin.PluginAuth + ", 聯繫方式 " +
                        plugin.PluginCont);
                    try
                    {
                        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                        using (
                            StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬插件" + plugin.PluginName + "錯誤報告.txt")
                            )
                        {
                            outfile.WriteLine("請有空發給聯繫方式 " + plugin.PluginCont + " 謝謝");
                            outfile.WriteLine(DateTime.Now + " " + plugin.PluginName + " " + plugin.PluginVer);
                            outfile.Write(ex.ToString());
                        }

                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    MessageBox.Show(
               "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給 copyliu@gmail.com ");
                    try
                    {
                        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                        using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬錯誤報告.txt"))
                        {
                            outfile.WriteLine("請有空發給 copyliu@gmail.com 謝謝");
                            outfile.WriteLine(DateTime.Now + "");
                            outfile.Write(ex.ToString());
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
           
        }
        public static void ReleaseMemory(bool removePages)
        {
            // release any unused pages
            // making the numbers look good in task manager
            // this is totally nonsense in programming
            // but good for those users who care
            // making them happier with their everyday life
            // which is part of user experience
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            if (removePages)
            {
                // as some users have pointed out
                // removing pages from working set will cause some IO
                // which lowered user experience for another group of users
                //
                // so we do 2 more things here to satisfy them:
                // 1. only remove pages once when configuration is changed
                // 2. add more comments here to tell users that calling
                //    this function will not be more frequent than
                //    IM apps writing chat logs, or web browsers writing cache files
                //    if they're so concerned about their disk, they should
                //    uninstall all IM apps and web browsers
                //
                // please open an issue if you're worried about anything else in your computer
                // no matter it's GPU performance, monitor contrast, audio fidelity
                // or anything else in the task manager
                // we'll do as much as we can to help you
                //
                // just kidding
                WINAPI.ReleasePages(Process.GetCurrentProcess().Handle);
            }
        }
    }
}