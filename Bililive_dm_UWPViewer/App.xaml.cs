using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Gaming.XboxGameBar;

namespace Bililive_dm_UWPViewer;

/// <summary>
///     提供特定于应用程序的行为，以补充默认的应用程序类。
/// </summary>
sealed partial class App : Application
{
    public static ThemeSetting ThemeSetting = new();

    internal static Settings Settings = new();
    private XboxGameBarWidget widget1;


    /// <summary>
    ///     初始化单一实例应用程序对象。这是执行的创作代码的第一行，
    ///     已执行，逻辑上等同于 main() 或 WinMain()。
    /// </summary>
    public App()
    {
        InitializeComponent();
        ThemeSetting.Theme = Current.RequestedTheme == ApplicationTheme.Dark
            ? ElementTheme.Dark
            : ElementTheme.Light;
        Suspending += OnSuspending;
    }

    /// <summary>
    ///     在应用程序由最终用户正常启动时进行调用。
    ///     将在启动应用程序以打开特定文件等情况下使用。
    /// </summary>
    /// <param name="e">有关启动请求和过程的详细信息。</param>
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        var rootFrame = Window.Current.Content as Frame;

        // 不要在窗口已包含内容时重复应用程序初始化，
        // 只需确保窗口处于活动状态
        if (rootFrame == null)
        {
            // 创建要充当导航上下文的框架，并导航到第一页
            rootFrame = new Frame();

            rootFrame.NavigationFailed += OnNavigationFailed;

            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                //TODO: 从之前挂起的应用程序加载状态
            }

            // 将框架放在当前窗口中
            Window.Current.Content = rootFrame;
        }

        if (e.PrelaunchActivated == false)
        {
            if (rootFrame.Content == null)
                // 当导航堆栈尚未还原时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 参数
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            // 确保当前窗口处于活动状态
            Window.Current.Activate();
        }
    }

    /// <summary>
    ///     导航到特定页失败时调用
    /// </summary>
    /// <param name="sender">导航失败的框架</param>
    /// <param name="e">有关导航失败的详细信息</param>
    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    /// <summary>
    ///     在将要挂起应用程序执行时调用。  在不知道应用程序
    ///     无需知道应用程序会被终止还是会恢复，
    ///     并让内存内容保持不变。
    /// </summary>
    /// <param name="sender">挂起的请求的源。</param>
    /// <param name="e">有关挂起请求的详细信息。</param>
    private void OnSuspending(object sender, SuspendingEventArgs e)
    {
        var deferral = e.SuspendingOperation.GetDeferral();
        //TODO: 保存应用程序状态并停止任何后台活动
        deferral.Complete();
    }


    protected override void OnActivated(IActivatedEventArgs args)
    {
        XboxGameBarWidgetActivatedEventArgs widgetArgs = null;
        if (args.Kind == ActivationKind.Protocol)
        {
            var protocolArgs = args as IProtocolActivatedEventArgs;
            var scheme = protocolArgs.Uri.Scheme;
            if (scheme.Equals("ms-gamebarwidget")) widgetArgs = args as XboxGameBarWidgetActivatedEventArgs;
        }

        if (widgetArgs != null)
        {
            //
            // Activation Notes:
            //
            //    If IsLaunchActivation is true, this is Game Bar launching a new instance
            // of our widget. This means we have a NEW CoreWindow with corresponding UI
            // dispatcher, and we MUST create and hold onto a new XboxGameBarWidget.
            //
            // Otherwise this is a subsequent activation coming from Game Bar. We MUST
            // continue to hold the XboxGameBarWidget created during initial activation
            // and ignore this repeat activation, or just observe the URI command here and act 
            // accordingly.  It is ok to perform a navigate on the root frame to switch 
            // views/pages if needed.  Game Bar lets us control the URI for sending widget to
            // widget commands or receiving a command from another non-widget process. 
            //
            // Important Cleanup Notes:
            //    When our widget is closed--by Game Bar or us calling XboxGameBarWidget.Close()-,
            // the CoreWindow will get a closed event.  We can register for Window.Closed
            // event to know when our partucular widget has shutdown, and cleanup accordingly.
            //
            // NOTE: If a widget's CoreWindow is the LAST CoreWindow being closed for the process
            // then we won't get the Window.Closed event.  However, we will get the OnSuspending
            // call and can use that for cleanup.
            //
            if (widgetArgs.IsLaunchActivation)
            {
                var rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;

                // Create Game Bar widget object which bootstraps the connection with Game Bar
                widget1 = new XboxGameBarWidget(
                    widgetArgs,
                    Window.Current.CoreWindow,
                    rootFrame);
                rootFrame.Navigate(typeof(Widget1), widget1);

                Window.Current.Closed += Widget1Window_Closed;

                Window.Current.Activate();
            }
            // You can perform whatever behavior you need based on the URI payload.
        }
    }

    private void Widget1Window_Closed(object sender, CoreWindowEventArgs e)
    {
        widget1 = null;
        Window.Current.Closed -= Widget1Window_Closed;
    }
}