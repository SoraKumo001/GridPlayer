using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GridPlayer
{
    /// <summary>
    /// MediaPlayer.xaml の相互作用ロジック
    /// </summary>
    public partial class MediaPlayer : UserControl
    {
        public event EventHandler mediaStop = (sender, e) => { };
        public event EventHandler mediaOpen = (sender, e) => { };
        public DispatcherTimer timer = new();
        public MediaElement media { get { return mediaElement; } }
        public MediaPlayer()
        {
            InitializeComponent();
            var settings = ((App)Application.Current).settings;
            DataContext = settings.appStatus;

        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            mediaController.Visibility = Visibility.Hidden;
        }
        public void play(string path)
        {
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.Source = new Uri(path);
            mediaController.setMedia(mediaElement);
            mediaController.mediaStop += _mediaStop;
        }
        private void _mediaStop(object? sender, EventArgs e)
        {
            mediaStop.Invoke(this, EventArgs.Empty);
        }
        public string Path
        {
            get { return mediaElement.Source.LocalPath; }
        }
        public double Position
        {
            get { return mediaElement.Position.TotalSeconds; }
            set { mediaElement.Position = TimeSpan.FromSeconds(value); }

        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            mediaController.Visibility = Visibility.Visible;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Tick += (object? sender, EventArgs e) =>
            {
                mediaController.Visibility = Visibility.Hidden;
                timer.Stop();
            };
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            mediaOpen.Invoke(this, e);
        }
    }
}
