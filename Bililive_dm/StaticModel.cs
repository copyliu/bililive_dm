using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bililive_dm.Annotations;

namespace Bililive_dm
{
    public class StaticModel:INotifyPropertyChanged
    {
        public StaticModel()
        {
            UserSet=new HashSet<string>(    );
        }
        public long DanmakuCountRaw
        {
            get { return _danmakuCountRaw; }
            set
            {
                if (value == _danmakuCountRaw) return;
                _danmakuCountRaw = value;
                OnPropertyChanged();
            }
        }

        public long DanmakuCountShow

        {
            get { return _danmakuCountShow; }
            set
            {
                if (value == _danmakuCountShow) return;
                _danmakuCountShow = value;
                OnPropertyChanged();
            }
        }

        private HashSet<string> UserSet;
        private long _danmakuCountRaw;
        private long _danmakuCountShow;
        private long _userCount;

        public long UserCount
        {
            get { return UserSet.Count; }
          
        }

        public void AddUser(string user)
        {
            UserSet.Add(user);
            OnPropertyChanged(nameof(UserCount));
        }

        public event PropertyChangedEventHandler PropertyChanged;

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