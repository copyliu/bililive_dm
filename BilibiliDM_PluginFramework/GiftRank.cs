using System;
using System.ComponentModel;
using BilibiliDM_PluginFramework.Annotations;

namespace BilibiliDM_PluginFramework
{
    public class GiftRank : INotifyPropertyChanged
    {
        private decimal _coin;
        private int _uid;
        private string _uid_str;
        private long _uidLong;
        private string _userName;

        /// <summary>
        ///     用戶名
        /// </summary>
        public string UserName
        {
            get => _userName;
            set
            {
                if (value == _userName) return;
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        /// <summary>
        ///     花銷
        /// </summary>
        public decimal coin
        {
            get => _coin;
            set
            {
                if (value == _coin) return;
                _coin = value;
                OnPropertyChanged(nameof(coin));
            }
        }

        /// <summary>
        ///     UID 弃用
        /// </summary>
        [Obsolete("由于B站开始使用超长UID, 此字段定义已无法满足, 在int范围内的UID会继续赋值, 超范围会赋值为-1, 请使用uid_long和uid_str")]

        public int uid
        {
            get => _uid;
            set
            {
                if (value == _uid) return;
                _uid = value;
                OnPropertyChanged(nameof(uid));
            }
        }

        public long uid_long
        {
            get => _uidLong;
            set
            {
                if (value == _uidLong) return;
                _uidLong = value;
                OnPropertyChanged(nameof(uid_long));
            }
        }

        /// <summary>
        ///     UID
        /// </summary>
        public string uid_str
        {
            get => _uid_str;
            set
            {
                if (value == _uid_str) return;
                _uid_str = value;
                OnPropertyChanged(nameof(uid_str));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}