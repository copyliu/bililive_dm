using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bililive_dm
{
    public class StyledWindow: Window
    {
        public StyledWindow()
        {
            SetResourceReference(StyleProperty, typeof(Window));
        }
    }
}
