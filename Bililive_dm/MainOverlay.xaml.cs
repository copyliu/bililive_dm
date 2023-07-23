using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;

namespace Bililive_dm
{
    using static WINAPI.USER32;

    /// <summary>
    ///     MainOverlay.xaml 的互動邏輯
    /// </summary>
    public partial class MainOverlay : Window
    {
        public MainOverlay()
        {
            InitializeComponent();
            Topmost = true;
            // 在此點下方插入建立物件所需的程式碼。
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // http://stackoverflow.com/a/551847
            var wndHelper = new WindowInteropHelper(this);
            var exStyles = GetExtendedWindowStyles(wndHelper.Handle);
            exStyles |= ExtendedWindowStyles.ToolWindow;
            SetExtendedWindowStyles(wndHelper.Handle, exStyles);
            SetWindowAffinity();
        }

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetWindowAffinity();
        }

        private void SetWindowAffinity()
        {
            var wndHelper = new WindowInteropHelper(this);
            SetWindowDisplayAffinity(wndHelper.Handle,
                Store.DisplayAffinity ? WindowDisplayAffinity.ExcludeFromCapture : 0);
        }
    }
}