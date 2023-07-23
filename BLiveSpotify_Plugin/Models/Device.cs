using System.Collections.Generic;

namespace BLiveSpotify_Plugin.Models
{
    internal class Device
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    internal class PlayDeviceResponse
    {
        public List<Device> devices { get; set; }
    }

    internal class SearchResponse
    {
        public TrackResponse tracks { get; set; }

        public class Track
        {
            public string id { get; set; }
            public string name { get; set; }
            public List<Artist> artists { get; set; }
            public string uri { get; set; }
        }

        public class Artist
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class TrackResponse
        {
            public List<Track> items { get; set; }
        }
    }
}