using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace GridPlayer
{
    public class MediaListViewModel : INotifyPropertyChanged
    {
        private readonly Settings _settings;
        private MediaDataList? _selectedPlaylist;
        private string _searchQuery = "";

        public ObservableCollection<MediaDataList> Playlists => _settings.mediaList;

        public MediaDataList? SelectedPlaylist
        {
            get => _selectedPlaylist;
            set
            {
                if (_selectedPlaylist != value)
                {
                    _selectedPlaylist = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredMediaData));
                }
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredMediaData));
                }
            }
        }

        public IEnumerable<MediaData> FilteredMediaData
        {
            get
            {
                if (SelectedPlaylist == null) return Enumerable.Empty<MediaData>();
                if (string.IsNullOrWhiteSpace(SearchQuery)) return SelectedPlaylist.mediaData;

                var query = SearchQuery.ToLower();
                return SelectedPlaylist.mediaData.Where(d => d.path.ToLower().Contains(query));
            }
        }

        public ICommand CopyPlaylistCommand { get; }
        public ICommand DeletePlaylistCommand { get; }
        public ICommand DeleteMediaDataCommand { get; }
        public ICommand ApplyPlaylistCommand { get; }

        public MediaListViewModel()
        {
            _settings = ((App)System.Windows.Application.Current).settings;

            CopyPlaylistCommand = new RelayCommand(_ => CopyPlaylist());
            DeletePlaylistCommand = new RelayCommand(p => DeletePlaylist(p as MediaDataList), p => CanDeletePlaylist(p as MediaDataList));
            DeleteMediaDataCommand = new RelayCommand(items => DeleteMediaData(items as System.Collections.IList));
            ApplyPlaylistCommand = new RelayCommand(p => ApplyPlaylist(p as MediaDataList));

            // Default selection
            if (Playlists.Count > 0)
            {
                SelectedPlaylist = Playlists[0];
            }
        }

        private void CopyPlaylist()
        {
            if (Playlists.Count > 0)
            {
                var newList = new MediaDataList
                {
                    name = "medias",
                    mediaData = new ObservableCollection<MediaData>(Playlists[0].mediaData)
                };
                Playlists.Add(newList);
                SelectedPlaylist = newList;
            }
        }

        private bool CanDeletePlaylist(MediaDataList? playlist)
        {
            return playlist != null && Playlists.Count > 0 && Playlists[0] != playlist;
        }

        private void DeletePlaylist(MediaDataList? playlist)
        {
            if (playlist != null && CanDeletePlaylist(playlist))
            {
                Playlists.Remove(playlist);
                if (SelectedPlaylist == playlist)
                {
                    SelectedPlaylist = Playlists.FirstOrDefault();
                }
            }
        }

        private void DeleteMediaData(System.Collections.IList? items)
        {
            if (items == null || SelectedPlaylist == null) return;

            var targetList = SelectedPlaylist.mediaData;
            var toDelete = items.Cast<MediaData>().ToList();
            foreach (var item in toDelete)
            {
                targetList.Remove(item);
            }
            OnPropertyChanged(nameof(FilteredMediaData));
        }

        private void ApplyPlaylist(MediaDataList? playlist)
        {
            if (playlist != null && Playlists.Count > 0)
            {
                var mainPlaylist = Playlists[0];
                mainPlaylist.mediaData.Clear();
                foreach (var d in playlist.mediaData)
                {
                    mainPlaylist.mediaData.Add(d);
                }
                SelectedPlaylist = mainPlaylist;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
