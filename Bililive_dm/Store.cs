using System;
using System.IO;
using System.Net;
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
    }

    public static class Utils
    {
        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
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
    }
}