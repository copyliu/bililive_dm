using System;
using System.Globalization;
using System.Windows.Data;

namespace Bililive_dm
{
    public class PluginStatusConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if ((bool) value == true)
            {
                return "已啟用";
            }
            else
            {
                return "未啟用";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}