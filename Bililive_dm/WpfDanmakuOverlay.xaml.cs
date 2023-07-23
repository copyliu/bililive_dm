using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Bililive_dm
{
    using static WINAPI.USER32;

    /// <summary>
    ///     WpfDanmakuOverlay.xaml 的互動邏輯
    /// </summary>
    public partial class WpfDanmakuOverlay : Window, IDanmakuWindow
    {
        public WpfDanmakuOverlay()
        {
            InitializeComponent();

            Deactivated += Overlay_Deactivated;
            Background = Brushes.Transparent;
            SourceInitialized += delegate
            {
                var hWnd = new WindowInteropHelper(this).Handle;
                var exStyles = GetExtendedWindowStyles(hWnd);
                SetExtendedWindowStyles(hWnd,
                    exStyles | ExtendedWindowStyles.Transparent | ExtendedWindowStyles.ToolWindow);
                SetMonitor(Store.FullScreenMonitor);
            };
            ShowInTaskbar = false;
            Topmost = true;
            Top = SystemParameters.WorkArea.Top;
            Left = SystemParameters.WorkArea.Left;
            Width = SystemParameters.WorkArea.Width;
            Height = 550;
        }

        void IDisposable.Dispose()
        {
            // do nothing
        }

        void IDanmakuWindow.Show()
        {
            this.Show();
        }

        void IDanmakuWindow.Close()
        {
            this.Close();
        }

        void IDanmakuWindow.ForceTopmost()
        {
            Topmost = false;
            Topmost = true;
        }

        void IDanmakuWindow.AddDanmaku(DanmakuType type, string comment, uint color)
        {
            if (this.CheckAccess())
                //<Storyboard x:Key="Storyboard1">
                //			<ThicknessAnimationUsingKeyFrames Storyboard.TargetProperty="(FrameworkElement.Margin)" Storyboard.TargetName="fullScreenDanmaku">
                //				<EasingThicknessKeyFrame KeyTime="0" Value="3,0,0,0"/>
                //				<EasingThicknessKeyFrame KeyTime="0:0:1.9" Value="220,0,0,0"/>
                //			</ThicknessAnimationUsingKeyFrames>
                //		</Storyboard>
                lock (LayoutRoot.Children)
                {
                    var v = new FullScreenDanmaku();
                    v.Text.Text = comment;
                    v.ChangeHeight();
                    var wd = v.Text.DesiredSize.Width;

                    var dd = new Dictionary<double, bool>();
                    dd.Add(0, true);
                    foreach (var child in LayoutRoot.Children)
                        if (child is FullScreenDanmaku)
                        {
                            var c = child as FullScreenDanmaku;
                            if (!dd.ContainsKey(Convert.ToInt32(c.Margin.Top)))
                                dd.Add(Convert.ToInt32(c.Margin.Top), true);
                            if (c.Margin.Left > SystemParameters.PrimaryScreenWidth - wd - 50)
                                dd[Convert.ToInt32(c.Margin.Top)] = false;
                        }

                    double top;
                    if (dd.All(p => p.Value == false))
                        top = dd.Max(p => p.Key) + v.Text.DesiredSize.Height;
                    else
                        top = dd.Where(p => p.Value).Min(p => p.Key);
                    // v.Height = v.Text.DesiredSize.Height;
                    // v.Width = v.Text.DesiredSize.Width;
                    var s = new Storyboard();
                    var duration =
                        new Duration(
                            TimeSpan.FromTicks(Convert.ToInt64((SystemParameters.PrimaryScreenWidth + wd) /
                                Store.FullOverlayEffect1 * TimeSpan.TicksPerSecond)));
                    var f =
                        new ThicknessAnimation(new Thickness(SystemParameters.PrimaryScreenWidth, top, 0, 0),
                            new Thickness(-wd, top, 0, 0), duration);
                    s.Children.Add(f);
                    s.Duration = duration;
                    Storyboard.SetTarget(f, v);
                    Storyboard.SetTargetProperty(f, new PropertyPath("(FrameworkElement.Margin)"));
                    LayoutRoot.Children.Add(v);
                    s.Completed += s_Completed;
                    s.Begin();
                }
            else
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                    () => (this as IDanmakuWindow).AddDanmaku(type, comment, color))
                );
        }

        public void SetMonitor(string deviceName)
        {
            var s = Screen.AllScreens.FirstOrDefault(p => p.DeviceName == deviceName) ?? Screen.PrimaryScreen;
            var r = s.WorkingArea;
            Top = r.Top;
            Left = r.Left;
            Width = r.Width;
        }

        void IDanmakuWindow.OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var wndHelper = new WindowInteropHelper(this);
            SetWindowDisplayAffinity(wndHelper.Handle,
                Store.DisplayAffinity ? WindowDisplayAffinity.ExcludeFromCapture : 0);


            if (e.PropertyName == nameof(Store.FullScreenMonitor)) SetMonitor(Store.FullScreenMonitor);
            // ignore
        }

        private void s_Completed(object sender, EventArgs e)
        {
            var s = sender as ClockGroup;
            if (s == null) return;
            var c = Storyboard.GetTarget(s.Children[0].Timeline) as FullScreenDanmaku;
            if (c != null) LayoutRoot.Children.Remove(c);
        }

        private void Overlay_Deactivated(object sender, EventArgs e)
        {
            if (sender is WpfDanmakuOverlay) (sender as WpfDanmakuOverlay).Topmost = true;
        }
    }
}