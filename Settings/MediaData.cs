using System.Collections.ObjectModel;

namespace GridPlayer
{
    public class MediaDataList
    {
        public string name { get; set; } = "";
        public ObservableCollection<MediaData> mediaData { get; set; } = new();
    }
    public class MediaData
    {
        public MediaData(string path, double position)
        {
            this.path = path;
            this.position = position;
        }
        public string path { get; set; }
        public double position { get; set; }
    }
}
