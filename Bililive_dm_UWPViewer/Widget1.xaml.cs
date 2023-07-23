using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Gaming.XboxGameBar;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Bililive_dm_UWPViewer;

public class Model
{
    public string User { get; set; }
    public string Comment { get; set; }
    public uint? UserCount { get; set; }
}

public enum Build
{
    Unknown = 0,
    Threshold1 = 1507, // 10240
    Threshold2 = 1511, // 10586
    Anniversary = 1607, // 14393 Redstone 1
    Creators = 1703, // 15063 Redstone 2
    FallCreators = 1709 // 16299 Redstone 3
}

/// <summary>
///     可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class Widget1 : Page
{
    public ObservableCollection<Model> modellist = new();
    private double requestedOpacity = 1;
    private XboxGameBarWidget widget;

    public Widget1()
    {
        InitializeComponent();

        ListView.ItemsSource = modellist;
        modellist.Add(new Model
        {
            Comment = $"提词板版本号 {GetAppVersion()}",
            User = "提示"
        });
        modellist.Add(new Model
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

    private SolidColorBrush widgetBackgroundBrush { get; set; }
    private SolidColorBrush TextBrush { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // you will need access to the XboxGameBarWidget, in this case it was passed as a parameter when navigating to the widget page, your implementation may differ.
        widget = e.Parameter as XboxGameBarWidget;

        // subscribe for RequestedOpacityChanged events
        if (widget != null)
        {
            widget.RequestedOpacityChanged += Widget_RequestedOpacityChanged;
            widget.RequestedThemeChanged += WidgetOnRequestedThemeChanged;
            widget.ClickThroughEnabledChanged += WidgetOnClickThroughEnabledChanged;
        }
    }

    private void WidgetOnClickThroughEnabledChanged(XboxGameBarWidget sender, object args)
    {
        App.Settings.ClickThroughEnabled = widget.ClickThroughEnabled;
    }

    private async void WidgetOnRequestedThemeChanged(XboxGameBarWidget sender, object args)
    {
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, SetColor);
    }

    private void SetColor()
    {
        App.ThemeSetting.Theme = widget.RequestedTheme;
        RequestedTheme = widget.RequestedTheme;
        var color = new SolidColorBrush(App.ThemeSetting.widgetBackgroundBrush.Color);
        color.Opacity = requestedOpacity;
        rootGrid.Background = color;
        Tb.Foreground = App.ThemeSetting.TextBrush;
    }

    private async void Widget_RequestedOpacityChanged(XboxGameBarWidget sender, object args)
    {
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            // adjust the opacity of your background as appropriate
            requestedOpacity = widget.RequestedOpacity;
            SetColor();
        });
    }

    public static string GetAppVersion()
    {
        var package = Package.Current;
        var packageId = package.Id;
        var version = packageId.Version;

        return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
    }

    private async Task TestTask()
    {
        while (true)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { AddLine(new Model { User = "test", Comment = DateTime.Now + "" }); });

            await Task.Delay(TimeSpan.FromSeconds(0.01));
        }
    }

    private void AddLine(Model m)
    {
        modellist.Add(m);
        if (modellist.Count > 5) modellist.RemoveAt(0);

        ListView.ScrollIntoView(m);
    }

    private async Task ConnectTask()
    {
        while (true)
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", @"BiliLive_DM_PIPE", PipeDirection.In))
                {
                    try
                    {
                        await pipeClient.ConnectAsync();
                        // pipeClient.ReadMode = PipeTransmissionMode.Message;
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            AddLine(new Model
                            {
                                User = "提示",
                                Comment = "检测到弹幕姬! 对接成功!"
                            });
                        });
                        await foreach (var m in GetList(pipeClient))
                            if (m.UserCount.HasValue)
                                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                    () => { Tb.Text = $"當前氣人值 : {m.UserCount}"; });
                            else
                                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { AddLine(m); });
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            AddLine(new Model
                            {
                                User = "提示",
                                Comment = "弹幕姬退出了."
                            });
                        });
                    }
                    catch (Exception e)
                    {
                        if (pipeClient.IsConnected) pipeClient.Close();

                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            AddLine(new Model
                            {
                                User = "提示",
                                Comment = "弹幕姬断开了, 开发者参考信息 " + e.Message
                            });
                        });
                    }
                }
            }
            catch (Exception e)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AddLine(new Model
                    {
                        User = "提示",
                        Comment = "弹幕姬断开了, 开发者参考信息 " + e.Message
                    });
                });
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

    private async void Setting_OnClick(object sender, RoutedEventArgs e)
    {
        var p = new SettingsDialog();
        p.DataContext = App.Settings;

        var T = await p.ShowAsync();
        //事后處理
    }
}