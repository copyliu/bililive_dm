using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace BilibiliDM_PluginFramework
{
    public class GiftRank:INotifyPropertyChanged
    {
        private string _userName;
        private decimal _coin;
        private int _uid;
        /// <summary>
        /// 用戶名
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == _userName) return;
                _userName = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 花銷
        /// </summary>
        public decimal coin
        {
            get { return _coin; }
            set
            {
                if (value == _coin) return;
                _coin = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// UID
        /// </summary>
        public int uid
        {
            get { return _uid; }
            set
            {
                if (value == _uid) return;
                _uid = value;
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