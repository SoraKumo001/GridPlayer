using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


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
            foreach (var file in dropFiles)
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
                if (index > 0)
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
            var xCount = 0;
            var yCount = 0;
            var max = 0.0;
            for (var y = 1; y <= count; y++)
            {
                for (var x = 1; x <= count; x++)
                {
                    if (x * y < count)
                        continue;
                    var w = grid.ActualWidth / x;
                    var h = grid.ActualHeight / y;
                    var r = w / h / this.ratio;
                    var v = r < 1.0 ? w * h * r : w * h / r;
                    if (v > max)
                    {
                        max = v;
                        yCount = y;
                        xCount = x;
                    }

                }
            }
            while (count > 0)
            {
                var over = xCount * yCount - count;
                if (over >= xCount)
                    --yCount;
                else if (over >= yCount)
                    --xCount;
                else
                    break;
            }
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            for (var i = 0; i < xCount; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (var i = 0; i < yCount; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (var i = 0; i < count; i++)
            {
                var control = grid.Children[i];
                Grid.SetRow(control, i / (int)xCount);
                Grid.SetColumn(control, i % (int)xCount);
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

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
                    var media = player.media;
                    if (media != null && media.NaturalVideoWidth > 0 && media.NaturalVideoHeight > 0)
                    {
                        ratio += (double)media.NaturalVideoWidth / media.NaturalVideoHeight;
                        count++;
                    }
                }
                ratio /= count;
                if (ratio > 0)
                    this.ratio = ratio;
                layout();
            }
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                if (isFullScreen)
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
                isFullScreen = !isFullScreen;
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
    }
}
