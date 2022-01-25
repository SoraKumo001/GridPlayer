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
            var jsonString = File.ReadAllText(settingsPath + "/" + settingsFile);
            if (jsonString != null)
            {
                try
                {
                    var settings = JsonSerializer.Deserialize<Settings>(jsonString);
                    if (settings != null)
                    {
                        this.settings = settings;
                    }
                }
                catch (Exception er) { Debug.WriteLine(er); }
            }
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
            mediaPlayer.play(path);
            mediaPlayer.Position = position;
            grid.Children.Add(mediaPlayer);
            layout();
        }
        private void layout()
        {
            //16:9
            var width = grid.ActualWidth / 4;
            var height = grid.ActualHeight / 3;
            var ratio = width / height;
            var count = grid.Children.Count;
            var wCount = 1.0;
            var hCount = 1.0;
            while (wCount * hCount < count)
            {
                if (hCount / wCount * ratio > 1)
                    ++wCount;
                else
                    ++hCount;
            }
            var over = wCount * hCount - count;
            if (over >= wCount)
                --hCount;
            else if (over >= hCount)
                --wCount;
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            for (var i = 0; i < wCount; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (var i = 0; i < hCount; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (var i = 0; i < count; i++)
            {
                var control = grid.Children[i];
                Grid.SetRow(control, i / (int)wCount);
                Grid.SetColumn(control, i % (int)wCount);
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
