using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bililive_dm
{
    /// <summary>
    /// FullScreenDanmaku.xaml 的互動邏輯
    /// </summary>
    public partial class FullScreenDanmaku : UserControl
    {
        public FullScreenDanmaku()
        {
            this.InitializeComponent();
        }


        public void ChangeHeight()
        {
            this.Text.FontSize = Store.FullOverlayFontsize;
            this.Text.Measure(new Size(int.MaxValue, int.MaxValue));
        }
    }
}