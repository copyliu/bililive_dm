using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using BilibiliDM_PluginFramework;

namespace Bililive_dm
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32", EntryPoint = "SetDllDirectoryW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        public App()
        {
            AddArchSpecificDirectory();
            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            
        }

        private void AddArchSpecificDirectory()
        {
            string archPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           IntPtr.Size == 8 ? "x64" : "Win32");
            SetDllDirectory(archPath);
        }

        private void App_DispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給 copyliu@gmail.com ");
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬錯誤報告.txt"))
                {
                    outfile.WriteLine("請有空發給 copyliu@gmail.com 謝謝");
                    outfile.WriteLine(DateTime.Now +"");
                    outfile.Write(e.Exception.ToString());
                    outfile.WriteLine("-------插件列表--------");
                    foreach (var dmPlugin in Plugins)
                    {
                        outfile.WriteLine($"{dmPlugin.PluginName}\t{dmPlugin.PluginVer}\t{dmPlugin.PluginAuth}\t{dmPlugin.PluginCont}\t启用:{dmPlugin.Status}");
                    }


                }
            }
            catch (Exception)
            {
            }
        }

        public static  readonly ObservableCollection<DMPlugin> Plugins = new ObservableCollection<DMPlugin>();
    }
}