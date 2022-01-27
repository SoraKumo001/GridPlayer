using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


class MediaData
{
    public MediaData(string path, double position)
    {
        this.path = path;
        this.position = position;
    }
    public string path { get; set; }
    public double position { get; set; }
}
class WindowStatus
{
    public double width { get; set; } = 640;
    public double height { get; set; } = 480;
    public double x { get; set; } = 0;
    public double y { get; set; } = 0;
}

class Settings
{
    public List<MediaData> mediaDatas { get; set; } = new();
    public WindowStatus windowStatus { get; set; } = new();

}

namespace GridPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private bool isFullScreen = false;
        private string settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/GridPlayer";
        private string settingsFile = "Settings.json";
        private Settings? settings;
        private double ratio = 16.0 / 9.0;
        public MainWindow()
        {
            InitializeComponent();
            loadSettings();
        }
        private void saveSettings()
        {
            var settings = new Settings();

            var windowStatus = new WindowStatus();
            windowStatus.x = Left;
            windowStatus.y = Top;
            windowStatus.width = Width;
            windowStatus.height = Height;
            settings.windowStatus = windowStatus;


            var mediaDatas = new List<MediaData>();
            foreach (MediaPlayer media in grid.Children)
            {
                mediaDatas.Add(new MediaData(media.Path, media.Position));
            }
            settings.mediaDatas = mediaDatas;

            Directory.CreateDirectory(settingsPath);
            string jsonString = JsonSerializer.Serialize(settings);
            File.WriteAllText(settingsPath + "/" + settingsFile, jsonString);
        }
        private void loadSettings()
        {
            try
            {
                var jsonString = File.ReadAllText(settingsPath + "/" + settingsFile);
                if (jsonString != null)
                {

                    var settings = JsonSerializer.Deserialize<Settings>(jsonString);
                    if (settings != null)
                    {
                        this.settings = settings;
                    }
                }

            }
            catch (Exception) { }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (settings?.windowStatus != null)
            {
                var x = settings.windowStatus.x;
                var y = settings.windowStatus.y;
                Width = Math.Min(settings.windowStatus.width, SystemParameters.WorkArea.Width);
                Height = Math.Min(settings.windowStatus.height, SystemParameters.WorkArea.Height);

                Left = x + Width > SystemParameters.WorkArea.Width ? SystemParameters.WorkArea.Width - Width : x;
                Top = y + Height > SystemParameters.WorkArea.Height ? SystemParameters.WorkArea.Height - Height : y;
            }
        }
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
                addMedia(file, 0);
            }

        }
        private void addMedia(string path, double position)
        {
            var mediaPlayer = new MediaPlayer();
            mediaPlayer.mediaStop += mediaStop;
            mediaPlayer.mediaOpen += mediaOpen;
            mediaPlayer.play(path);
            mediaPlayer.Position = position;
            grid.Children.Add(mediaPlayer);
            layout();
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            layout();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


            if (settings?.mediaDatas != null)
            {
                foreach (var media in settings.mediaDatas)
                {
                    addMedia(media.path, media.position);
                }
            }

        }
        private void mediaStop(object? sender, EventArgs e)
        {
            if (sender != null)
            {
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveSettings();
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
    }
}
