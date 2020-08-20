using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Bililive_dm.Properties;

namespace Bililive_dm
{
    public class FontSizeValidationRule : ValidationRule
    {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            if (str == null)
            {
                return new ValidationResult(false, Resources.FontSizeValidationRule_Validate_不可为空);
            }
            float ftvalue;
            if (!float.TryParse(str,out ftvalue))
            {
                return new ValidationResult(false, Resources.FontSizeValidationRule_Validate_不是数字);
            }
            if (ftvalue < 0)
            {
                return new ValidationResult(false, Resources.FontSizeValidationRule_Validate_必须是正数);
            }
            return new ValidationResult(true, null);

        }
    }
    /// <summary>
    /// OptionDialog.xaml 的互動邏輯
    /// </summary>
    public partial class OptionDialog 
    {
        public OptionDialog()
        {
            this.InitializeComponent();
           
            this.ScreenSelect.ItemsSource = System.Windows.Forms.Screen.AllScreens.Select(p => p.DeviceName).ToList();
           


        }


       


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow) (Application.Current.MainWindow)).Test_OnClick(null, null);
        }

        private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
        {
            var a = (StoreModel) this.LayoutRoot.DataContext;
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
            a.SaveConfig();
        }

    }
}