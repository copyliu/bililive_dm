using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using BilibiliDM_PluginFramework;
using BililiveDebugPlugin.Annotations;

namespace BililiveDebugPlugin
{
    public sealed class MethodToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var methodName = parameter as string;
            if (value == null || methodName == null)
                return null;
            var methodInfo = value.GetType().GetMethod(methodName, new Type[0]);
            if (methodInfo == null)
                return null;
            return methodInfo.Invoke(value, new object[0]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
        }
    }

    public class DMItem
    {
        public string ItemName { get; set; }
        public DanmakuModel Model { get; set; }

        public override string ToString()
        {
            return ItemName;
        }
    }

    public class PluginDataContext : INotifyPropertyChanged
    {
        private ObservableCollection<DMItem> _dataList;
        private DanmakuModel _selected;


        public PluginDataContext()
        {
            DataList = new ObservableCollection<DMItem>();
        }

        public DMPlugin Plugin { get; set; }

        public ObservableCollection<DMItem> DataList
        {
            get => _dataList;
            set
            {
                if (Equals(value, _dataList)) return;
                _dataList = value;
                OnPropertyChanged();
            }
        }

        public DanmakuModel Selected
        {
            get => _selected;
            set
            {
                if (Equals(value, _selected)) return;
                _selected = value;
                OnPropertyChanged();
            }
        }

        public bool Status
        {
            get => Plugin?.Status == true;
            set
            {
                if (Plugin == null) return;

                if (value)
                    Plugin.Start();
                else
                    Plugin.Stop();
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}