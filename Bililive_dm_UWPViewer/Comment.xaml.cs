using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Bililive_dm_UWPViewer
{
    public sealed partial class Comment : UserControl
    {
        public Brush texBrush { get; set; }

        public Comment()
        {
            this.InitializeComponent();
            App.ThemeSetting.PropertyChanged += ThemeSettingOnPropertyChanged;
            // SolidColorBrush brush = (SolidColorBrush)this.Resources["RunForeground1"];
            this.r1.Foreground = App.ThemeSetting.TextBrush;
            this.Text.Foreground = App.ThemeSetting.TextBrush;
            App.Settings.PropertyChanged += SettingsOnPropertyChanged;
            this.DataContextChanged+= Comment_DataContextChanged;
            if (App.Settings.HideWhenTrans)
            {
                this.Hide.Begin();
            }
            else
            {
                this.Hide.Stop();
                this.Hide.Seek(TimeSpan.Zero);
            }
        }

        private void Comment_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (App.Settings.HideWhenTrans  && App.Settings.ClickThroughEnabled)
            {
                this.Hide.Seek(TimeSpan.Zero);
                this.Hide.Begin();
            }
            else
            {
                this.Hide.Stop();
                this.Hide.Seek(TimeSpan.Zero);
            }
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (App.Settings.HideWhenTrans && App.Settings.ClickThroughEnabled)
                {
                    this.Hide.Seek(TimeSpan.Zero);
                    this.Hide.Begin();
                }
                else
                {
                    this.Hide.Stop();
                    this.Hide.Seek(TimeSpan.Zero);
                }
            });


        }

        private void ThemeSettingOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThemeSetting.Theme))
            {

                this.r1.Foreground = App.ThemeSetting.TextBrush;
                this.Text.Foreground = App.ThemeSetting.TextBrush;
            }
        }
    }
}

