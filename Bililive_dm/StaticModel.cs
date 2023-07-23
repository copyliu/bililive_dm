using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bililive_dm.Annotations;

namespace Bililive_dm
{
    public class StaticModel : INotifyPropertyChanged
    {
        private long _danmakuCountRaw;
        private long _danmakuCountShow;
        private long _userCount;

        private readonly HashSet<string> UserSet;

        public StaticModel()
        {
            UserSet = new HashSet<string>();
        }

        public long DanmakuCountRaw
        {
            get => _danmakuCountRaw;
            set
            {
                if (value == _danmakuCountRaw) return;
                _danmakuCountRaw = value;
                OnPropertyChanged();
            }
        }

        public long DanmakuCountShow

        {
            get => _danmakuCountShow;
            set
            {
                if (value == _danmakuCountShow) return;
                _danmakuCountShow = value;
                OnPropertyChanged();
            }
        }

        public long UserCount => UserSet.Count;

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddUser(string user)
        {
            UserSet.Add(user);
            OnPropertyChanged(nameof(UserCount));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ClearUser()
        {
            UserSet.Clear();
        }
    }
}