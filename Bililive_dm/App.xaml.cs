using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace Bililive_dm
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(System.Windows.Application.Current.MainWindow, "遇到了不明錯誤: 日誌已經保存在桌面, 請有空發給 copyliu@gmail.com ");
            try
            {


                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


                using (StreamWriter outfile = new StreamWriter(path + @"\B站彈幕姬錯誤報告.txt"))
                {
                    outfile.WriteLine("請有空發給 copyliu@gmail.com 謝謝");
                    outfile.Write(e.Exception.ToString());
                }

            }
            catch (Exception)
            {



            }
        }
    }
}
