using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BilibiliDM_PluginFramework;
using BililiveAudioCmtPlayer.Annotations;

namespace BililiveAudioCmtPlayer
{
    public class DMItem
    {
        public string ItemName { get; set; }
        public DanmakuModel Model { get; set; }
        public override string ToString() => ItemName;
    }
    public class PluginDataContext : INotifyPropertyChanged
    {
        private DanmakuModel _selected;
        private ObservableCollection<DMItem> _dataList;


        public PluginDataContext()
        {
            this.DataList = new ObservableCollection<DMItem>();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                if (Plugin == null) { return; }

                if (value)
                {
                    this.Plugin.Start();
                }
                else
                {
                    this.Plugin.Stop();
                }
                OnPropertyChanged();
            }
        }
    }
}
