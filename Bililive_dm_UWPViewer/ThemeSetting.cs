using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Bililive_dm_UWPViewer.Annotations;

namespace Bililive_dm_UWPViewer
{
    public class ThemeSetting:INotifyPropertyChanged
    {
        private  ElementTheme _theme;

        public  ElementTheme Theme
        {
            get => _theme;
            set
            {
                if (value == _theme) return;
                _theme = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(widgetBackgroundBrush));
                OnPropertyChanged(nameof(TextBrush));
            }
        }

        public SolidColorBrush widgetBackgroundBrush
        {
            get
            {
                if (Theme == ElementTheme.Dark)
                {
                    return BlackBrush;
                }
                else
                {
                    return WhiteBrush;
                }
            }
        }

        public  SolidColorBrush TextBrush
        {
            get
            {
                if (Theme == ElementTheme.Dark)
                {
                    return WhiteBrush ;
                }
                else
                {
                    return BlackBrush;
                }
            }
        }

      private static readonly SolidColorBrush   BlackBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 38, 38, 38));
        private static readonly SolidColorBrush WhiteBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 219, 219, 219));


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
