using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BilibiliDM_PluginFramework;
using BililiveAudioCmtPlayer.Annotations;

namespace BililiveAudioCmtPlayer
{
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