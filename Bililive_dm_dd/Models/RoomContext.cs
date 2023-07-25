using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BilibiliDM_PluginFramework;
using BiliDMLib;

namespace Bililive_dm_dd.Models
{
    public class RoomContext:INotifyPropertyChanged
    {
        public long RoomId
        {
            get => _roomId;
            set => SetField(ref _roomId, value);
        }

        public string RoomTitle
        {
            get => _roomTitle;
            set => SetField(ref _roomTitle, value);
        }
        
        public long RoomViewerCount
        {
            get => _roomViewerCount;
            set => SetField(ref _roomViewerCount, value);
        }

        public bool Connected
        {
            get => _connected;
            set
            {
                SetField(ref _connected, value);
                OnPropertyChanged(nameof(ConnectedString));
            }
        }

        public string ConnectedString => _connected ? "ONLINE" : "NO SIGNAL";

        public ObservableCollection<string> MessageQueue
        {
            get => _messageQueue;
            set => SetField(ref _messageQueue, value);
        }

        private long _roomId;
        private string _roomTitle;
        private long _roomViewerCount;
        private bool _connected;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private BiliDMLib.DanmakuLoader _loader = new DanmakuLoader();
        private ObservableCollection<string> _messageQueue;

        public RoomContext()
        {
            this.MessageQueue = new ObservableCollection<string>();
            _loader.ReceivedDanmaku += (sender, args) =>
            {
                switch (args.Danmaku.MsgType)
                {
                    case MsgTypeEnum.Comment:
                        MessageQueue.Add(args.Danmaku.UserName + ":" + args.Danmaku.CommentText);
                        break;
                    
                }
            };
            _loader.ReceivedRoomCount += (sender, args) =>
            {
                this.RoomViewerCount = args.UserCount;
            };
            _loader.Disconnected += (sender, args) =>
            {
                this.Connected = false;
            };
        }
        public async Task Connect()
        {
            if (this._connected)
            {
                return;
            }
            await _loader.ConnectAsync((int)this.RoomId);
            this.Connected = true;

        }
    }
}