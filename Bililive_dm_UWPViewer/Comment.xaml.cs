using System;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Bililive_dm_UWPViewer;

public sealed partial class Comment : UserControl
{
    public Comment()
    {
        InitializeComponent();
        App.ThemeSetting.PropertyChanged += ThemeSettingOnPropertyChanged;
        // SolidColorBrush brush = (SolidColorBrush)this.Resources["RunForeground1"];
        r1.Foreground = App.ThemeSetting.TextBrush;
        Text.Foreground = App.ThemeSetting.TextBrush;
        App.Settings.PropertyChanged += SettingsOnPropertyChanged;
        DataContextChanged += Comment_DataContextChanged;
        if (App.Settings.HideWhenTrans)
        {
            Hide.Begin();
        }
        else
        {
            Hide.Stop();
            Hide.Seek(TimeSpan.Zero);
        }
    }

    public Brush texBrush { get; set; }

    private void Comment_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (App.Settings.HideWhenTrans && App.Settings.ClickThroughEnabled)
        {
            Hide.Seek(TimeSpan.Zero);
            Hide.Begin();
        }
        else
        {
            Hide.Stop();
            Hide.Seek(TimeSpan.Zero);
        }
    }

    private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            if (App.Settings.HideWhenTrans && App.Settings.ClickThroughEnabled)
            {
                Hide.Seek(TimeSpan.Zero);
                Hide.Begin();
            }
            else
            {
                Hide.Stop();
                Hide.Seek(TimeSpan.Zero);
            }
        });
    }

    private void ThemeSettingOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ThemeSetting.Theme))
        {
            r1.Foreground = App.ThemeSetting.TextBrush;
            Text.Foreground = App.ThemeSetting.TextBrush;
        }
    }
}