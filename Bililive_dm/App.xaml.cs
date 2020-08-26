using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using System.Reflection;
using System.Resources;
using System.IO.Compression;

namespace Bililive_dm
{
    using BilibiliDM_PluginFramework;

    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App: Application
    {
        internal Collection<ResourceDictionary> merged { get; private set; }
        internal ResourceDictionary AeroWin8 { get; private set; }

        public App()
        {

            AddArchSpecificDirectory();
            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            try
            {
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            }
            catch (ConfigurationErrorsException ex)
            {//重置修复错误的配置文件
                string filename = ex.Filename;
                File.Delete(filename);
                Bililive_dm.Properties.Settings.Default.Reload();

            }

            var culture = CultureInfo.GetCultureInfo(Bililive_dm.Properties.Settings.Default.lang);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
        }

        public static new App Current => (App)Application.Current;

        private void AddArchSpecificDirectory()
        {
            string archPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           IntPtr.Size == 8 ? "x64" : "Win32");
            WINAPI.SetDllDirectory(archPath);
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
                    outfile.WriteLine(DateTime.Now + "");
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

        public static readonly ObservableCollection<DMPlugin> Plugins = new ObservableCollection<DMPlugin>();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var ms = new MemoryStream();

            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                var name = new AssemblyName(args.Name);

                switch (name.Name)
                {
                    case "PresentationTheme.Aero":
                    case "PresentationTheme.Aero.Win8":
                        break;
                    default:
                        return null;
                }


                ms.Position = 0;
                using (var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Bililive_dm.Assets.{name.Name}.dll.gz"))
                using (var zs = new GZipStream(rs, CompressionMode.Decompress))
                    zs.CopyTo(ms);

                var data = new byte[ms.Position];
                ms.Position = 0;
                ms.Read(data, 0, data.Length);
                return Assembly.Load(data);
            };

            AeroWin8 = (ResourceDictionary)Resources["Win8"];


            merged = Resources.MergedDictionaries;
            merged.Add((ResourceDictionary)Resources["Default"]);
        }
    }
}