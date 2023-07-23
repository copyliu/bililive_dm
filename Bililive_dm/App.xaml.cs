using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using BilibiliDM_PluginFramework;
using Bililive_dm.Properties;

namespace Bililive_dm
{
    /// <summary>
    ///     App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        public static readonly ObservableCollection<DMPlugin> Plugins = new ObservableCollection<DMPlugin>();

        public App()
        {
            AddArchSpecificDirectory();
            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            try
            {
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            }
            catch (ConfigurationErrorsException ex)
            {
                //重置修复错误的配置文件
                var filename = ex.Filename;
                File.Delete(filename);
                Settings.Default.Reload();
            }

            var culture = CultureInfo.GetCultureInfo(Settings.Default.lang);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
        }

        internal Collection<ResourceDictionary> merged { get; private set; }

        public new static App Current => (App)Application.Current;

        private void AddArchSpecificDirectory()
        {
            var archPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
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
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                using (var outfile = new StreamWriter(path + @"\B站彈幕姬錯誤報告.txt"))
                {
                    outfile.WriteLine("請有空發給 copyliu@gmail.com 謝謝");
                    outfile.WriteLine(DateTime.Now + "");
                    outfile.Write(e.Exception.ToString());
                    outfile.WriteLine("-------插件列表--------");
                    foreach (var dmPlugin in Plugins)
                        outfile.WriteLine(
                            $"{dmPlugin.PluginName}\t{dmPlugin.PluginVer}\t{dmPlugin.PluginAuth}\t{dmPlugin.PluginCont}\t启用:{dmPlugin.Status}");
                }
            }
            catch (Exception)
            {
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            merged = Resources.MergedDictionaries;
            merged.Add((ResourceDictionary)Resources["Default"]);
        }
    }
}