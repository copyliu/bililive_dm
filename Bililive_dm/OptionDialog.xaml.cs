using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Bililive_dm.Properties;
using Application = System.Windows.Application;

namespace Bililive_dm
{
    public class FontSizeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            if (str == null) return new ValidationResult(false, Resources.FontSizeValidationRule_Validate_不可为空);
            float ftvalue;
            if (!float.TryParse(str, out ftvalue))
                return new ValidationResult(false, Resources.FontSizeValidationRule_Validate_不是数字);
            if (ftvalue < 0) return new ValidationResult(false, Resources.FontSizeValidationRule_Validate_必须是正数);
            return new ValidationResult(true, null);
        }
    }

    /// <summary>
    ///     OptionDialog.xaml 的互動邏輯
    /// </summary>
    public partial class OptionDialog
    {
        public OptionDialog()
        {
            InitializeComponent();

            ScreenSelect.ItemsSource = Screen.AllScreens.Select(p => p.DeviceName).ToList();
            FontFamilySelecter.ItemsSource = Fonts.SystemFontFamilies;
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Test_OnClick(null, null);
        }

        private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
        {
            var a = (StoreModel)LayoutRoot.DataContext;
            a.FullOverlayEffect1 = DefaultStore.FullOverlayEffect1;
            a.FullOverlayFontsize = DefaultStore.FullOverlayFontsize;
            a.MainOverlayEffect1 = DefaultStore.MainOverlayEffect1;
            a.MainOverlayEffect2 = DefaultStore.MainOverlayEffect2;
            a.MainOverlayEffect3 = DefaultStore.MainOverlayEffect3;
            a.MainOverlayEffect4 = DefaultStore.MainOverlayEffect4;
            a.MainOverlayFontsize = DefaultStore.MainOverlayFontsize;
            a.MainOverlayWidth = DefaultStore.MainOverlayWidth;
            a.MainOverlayXoffset = DefaultStore.MainOverlayXoffset;
            a.MainOverlayYoffset = DefaultStore.MainOverlayYoffset;
            a.WtfEngineEnabled = DefaultStore.WtfEngineEnabled;
            a.DisplayAffinity = DefaultStore.DisplayAffinity;
            a.FullScreenMonitor = DefaultStore.FullScreenMonitor;
            a.MainFontFamily = new FontFamily("Global User Interface");
            a.SaveConfig();
        }
    }
}