using System.Windows;
using Bililive_dm.Properties;

namespace Bililive_dm
{
    /// <summary>
    ///     LanguageSelector.xaml 的交互逻辑
    /// </summary>
    public partial class LanguageSelector : Window
    {
        public LanguageSelector()
        {
            InitializeComponent();

            switch (Settings.Default.lang)
            {
                case "en-US":
                    en.IsChecked = true;
                    break;
                case "ja-JP":
                    jp.IsChecked = true;
                    break;
                case "zh":
                default:
                    cn.IsChecked = true;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cn.IsChecked == true)
                Settings.Default.lang = "zh";
            else if (jp.IsChecked == true)
                Settings.Default.lang = "ja-JP";
            else if (en.IsChecked == true) Settings.Default.lang = "en-US";
            Settings.Default.Save();
            MessageBox.Show(this,
                "语言设定将在重启弹幕姬后生效. \n言語設定は再起動後に有効になります. \nLanguage settings will take effect after restart.");
            Close();
        }
    }
}