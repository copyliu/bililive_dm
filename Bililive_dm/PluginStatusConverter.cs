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
                return Properties.Resources.Plugin_DataGrid_Status_On;
            }
            else
            {
                return Properties.Resources.Plugin_DataGrid_Status_Off;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}