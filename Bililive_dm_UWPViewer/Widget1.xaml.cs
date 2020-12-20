using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bililive_dm_UWPViewer
{
    public class Model
    {
        public string User { get; set; }
        public string Comment { get; set; }
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Widget1 : Page
    {
      public ObservableCollection<Model> modellist=new();
        public Widget1()
        {
            this.InitializeComponent();
            this.ListView.ItemsSource = modellist;
            modellist.Add(new Model()
            {
                Comment = "請打開彈幕姬本體並連接到直播間",
                User = "提示"
            });
                
             //
             // pipeServer =
             //    new NamedPipeServerStream(@"LOCAL\\BiliLive_DM_PIPE", PipeDirection.In, 10,
             //        PipeTransmissionMode.Message, PipeOptions.None, 4096, 4096);
             var _ = ConnectTask();
             // var __ = TestTask();
        }

        async Task TestTask()
        {
            while (true)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AddLine(new Model() { User = "test", Comment = DateTime.Now + "" });
                });
               
                await Task.Delay(TimeSpan.FromSeconds(0.01));
            }
        }

        void AddLine(Model m)
        {
            modellist.Add(m);
            if (modellist.Count > 50)
            {
                modellist.RemoveAt(0);
            }
            this.ListView.ScrollIntoView(m);
        }
        
        async Task ConnectTask()
        {
            while (true)
            {
                using (var pipeClient = new NamedPipeClientStream(".", @"BiliLive_DM_PIPE", PipeDirection.In))
                {
                   
                    try
                    {
                        await pipeClient.ConnectAsync();
                        // pipeClient.ReadMode = PipeTransmissionMode.Message;

                       await foreach (var m in GetList(pipeClient))
                       {
                           await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                           {
                               AddLine(m);
                           });
                       }

                    }
                    catch (Exception e)
                    {
                        if (pipeClient.IsConnected)
                        {
                            pipeClient.Close();
                        }

                    }

                }



                await Task.Delay(TimeSpan.FromSeconds(1));
            }

        }


        private async IAsyncEnumerable<Model> GetList(NamedPipeClientStream pipeClient)
        {
            while (pipeClient.IsConnected)
            {
                string line;
                
                using var reader = new StreamReader(pipeClient);
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var item = JsonSerializer.Deserialize<Model>(line);
                    yield return item;

                       
                }
            }
           
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
