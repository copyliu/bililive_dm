using System;
using System.Globalization;
using System.Windows.Data;
using Bililive_dm.Properties;

namespace Bililive_dm
{
    public class PluginStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Resources.Plugin_DataGrid_Status_On;
            return Resources.Plugin_DataGrid_Status_Off;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}