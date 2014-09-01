using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bililive_dm
{
	/// <summary>
	/// DanmakuTextControl.xaml 的互動邏輯
	/// </summary>
	public partial class DanmakuTextControl : UserControl
	{
		public DanmakuTextControl()
		{
			this.InitializeComponent();
          //  this.Loaded += DanmakuTextControl_Loaded;
		}

	    public void ChangeHeight()
	    {
            this.TextBox.Measure(new Size(250, int.MaxValue));
	        var sb = (Storyboard) this.Resources["Storyboard1"];
            var kf1 = sb.Children[0] as DoubleAnimationUsingKeyFrames;
            kf1.KeyFrames[1].Value = this.TextBox.DesiredSize.Height;
           Storyboard.SetTarget(sb.Children[1],this);

	    }

        void DanmakuTextControl_Loaded(object sender, RoutedEventArgs e)
        {
//            this.Loaded-=DanmakuTextControl_Loaded;
//            this.BeginStoryboard((Storyboard)this.Resources["Storyboard1"]);
        }
	}
}