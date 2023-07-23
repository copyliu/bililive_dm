using System.ComponentModel;
using System.Runtime.CompilerServices;
using BLiveSpotify_Plugin.Annotations;

namespace BLiveSpotify_Plugin
{
    public class PlayDeviceModel
    {
        public string PlaylistId { get; set; }
        public string PlaylistName { get; set; }
    }

    public class MusicModel
    {
        public string MusicId { get; set; }
        public string MusicName { get; set; }
        public string MusicArtist { get; set; }
    }

    public class PluginDataContext : INotifyPropertyChanged
    {
        private PlayDeviceModel _selectedPlayList;


        public PlayDeviceModel SelectedPlayList
        {
            get => _selectedPlayList;
            set
            {
                if (Equals(value, _selectedPlayList)) return;

                var spotifyObj = Plugin.spotifyLib;
                if (spotifyObj != null)
                {
                    spotifyObj.playdevice = value?.PlaylistId;
                    spotifyObj.SaveConfig();
                    _selectedPlayList = value;
                }

                OnPropertyChanged();
            }
        }

        private bool IsLogin => !string.IsNullOrEmpty(Plugin.spotifyLib.refresh_token);

        public string LoginStatus => IsLogin ? "已登入" : "未登入";

        public BLiveSpotify_Plugin Plugin { get; set; }

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
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}