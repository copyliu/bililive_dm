﻿using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Bililive_dm_UWPViewer;

/// <summary>
///     可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        Widget1.modellist.Add(new Model
        {
            User = "提示",
            Comment = "建议在XBbo游戏工具条 (快捷键 Win+G ) 的小工具菜单中启动"
        });
    }
}