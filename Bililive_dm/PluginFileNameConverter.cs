using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using BilibiliDM_PluginFramework;

namespace Bililive_dm
{
    public class PluginFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DMPlugin plugin)
            {
                try
                {
                    return Path.GetFileName(plugin.GetType().Assembly.Location);
                }
                catch (Exception e)
                {
                    return "";
                } 
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}