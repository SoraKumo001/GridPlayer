using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace GridPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private bool isFullScreen = false;
        private Settings settings;
        private double ratio = 16.0 / 9.0;
        public MainWindow()
        {
            InitializeComponent();
            settings = ((App)Application.Current).settings;
            DataContext = settings.windowStatus;
        }
        private ObservableCollection<MediaData> mediaDatas { get { return settings.mediaList[0].mediaData; } }



        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
        }
        private void Window_Drop(object sender, DragEventArgs e)
        {
            var dropFiles = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];
            if (dropFiles == null) return;
            var mediaFiles = MediaFileHelper.GetAllMediaFiles(dropFiles);
            foreach (var file in mediaFiles)
            {
                mediaDatas.Add(new MediaData(file, 0));
            }

        }
        private void updateMedia()
        {
            var mediaPlayers = new MediaPlayer?[grid.Children.Count];
            grid.Children.CopyTo(mediaPlayers, 0);

            var newList = new List<MediaPlayer?>();
            foreach (var media in mediaDatas)
            {
                var index = Array.FindIndex(mediaPlayers, (v) => v != null && v.Path == media.path);
                if (index >= 0)
                {
                    newList.Add(mediaPlayers[index]);
                    mediaPlayers[index] = null;
                }
                else
                {
                    var mediaPlayer = createMedia(media.path, media.position);
                    newList.Add(mediaPlayer);
                }
            }
            for (var i = 0; i < newList.Count; i++)
            {
                if (i >= grid.Children.Count)
                {
                    grid.Children.Insert(i, newList[i]);
                }
                else
                {
                    var child = (MediaPlayer)grid.Children[i];
                    if (child.Path != newList[i]?.Path)
                    {
                        if (newList[i]?.Parent != null)
                            grid.Children.Remove(newList[i]);
                        grid.Children.Insert(i, newList[i]);
                    }
                }
            }
            if (grid.Children.Count > newList.Count)
            {
                grid.Children.RemoveRange(newList.Count, grid.Children.Count - newList.Count);
            }
            layout();
        }
        private MediaPlayer createMedia(string path, double position)
        {
            var mediaPlayer = new MediaPlayer();
            mediaPlayer.mediaStop += mediaStop;
            mediaPlayer.mediaOpen += mediaOpen;
            mediaPlayer.play(path);
            mediaPlayer.Position = position;
            return mediaPlayer;

        }
        private void layout()
        {
            var count = grid.Children.Count;
            if (count == 0) return;

            var dimensions = GridLayoutCalculator.CalculateOptimalGrid(count, grid.ActualWidth, grid.ActualHeight, ratio);

            // 2. Clear and recreate definitions
            grid.ColumnDefinitions.Clear();
            for (var i = 0; i < dimensions.Columns; i++) grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Clear();
            for (var i = 0; i < dimensions.Rows; i++) grid.RowDefinitions.Add(new RowDefinition());

            // 3. Place items with Span logic to fill gaps
            var placements = GridLayoutCalculator.CalculatePlacements(count, dimensions);
            for (var i = 0; i < count; i++)
            {
                var control = grid.Children[i];
                var placement = placements[i];

                Grid.SetRow(control, placement.Row);
                Grid.SetColumn(control, placement.Column);
                Grid.SetRowSpan(control, placement.RowSpan);
                Grid.SetColumnSpan(control, placement.ColumnSpan);
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set priority class: {ex.Message}");
            }

            mediaDatas.CollectionChanged += MediaDatas_CollectionChanged;
            updateMedia();
        }

        private void MediaDatas_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            updateMedia();
        }

        private void mediaStop(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                int i;
                for (i = 0; i < grid.Children.Count && grid.Children[i] != sender; i++) ;
                mediaDatas.RemoveAt(i);
                grid.Children.Remove((UIElement)sender);
                layout();
            }
        }
        private void mediaOpen(object? sender, EventArgs e)
        {

            if (grid.Children.Count > 0 && sender != null)
            {
                var ratio = 0.0;
                var count = 0;
                foreach (MediaPlayer player in grid.Children)
                {
                    if (player.NaturalWidth > 0 && player.NaturalHeight > 0)
                    {
                        ratio += player.NaturalWidth / player.NaturalHeight;
                        count++;
                    }
                }
                if (count > 0)
                {
                    ratio /= count;
                    this.ratio = ratio;
                    layout();
                }
            }
        }
        private void setScreenMode(bool isFullScreen)
        {
            if (isFullScreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
            this.isFullScreen = isFullScreen;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                setScreenMode(!isFullScreen);
            }

        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            appController.active();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            for (var i = 0; i < grid.Children.Count; i++)
            {
                settings.mediaList[0].mediaData[i].position = ((MediaPlayer)grid.Children[i]).Position;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            layout();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F:
                    setScreenMode(!isFullScreen);
                    break;
                case Key.Space:
                    bool isPlay = false;
                    foreach (MediaPlayer p in grid.Children)
                    {
                        if (p.IsPlaying)
                        {
                            isPlay = true;
                            break;
                        }
                    }
                    foreach (MediaPlayer p in grid.Children)
                    {
                        if (isPlay)
                        {
                            p.Pause();

                        }
                        else
                        {
                            p.Play();
                        }
                    }
                    break;
                case Key.Right:
                    foreach (MediaPlayer p in grid.Children)
                    {
                        if (p.HasDuration)
                            p.Position += 10;
                    }
                    break;
                case Key.Left:
                    foreach (MediaPlayer p in grid.Children)
                    {
                        if (p.HasDuration)
                            p.Position -= 10;
                    }
                    break;
                case Key.Up:
                    settings.appStatus.Volume = Math.Min(settings.appStatus.Volume + 0.1, 1);
                    break;
                case Key.Down:
                    settings.appStatus.Volume = Math.Max(settings.appStatus.Volume - 0.1, 0);
                    break;
            }
        }
    }
}
