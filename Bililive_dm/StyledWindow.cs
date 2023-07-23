using System.Windows;

namespace Bililive_dm
{
    public class StyledWindow : Window
    {
        public StyledWindow()
        {
            SetResourceReference(StyleProperty, typeof(Window));
        }
    }
}