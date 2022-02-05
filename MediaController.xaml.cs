using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GridPlayer
{
    /// <summary>
    /// MediaController.xaml の相互作用ロジック
    /// </summary>
    public partial class MediaController : UserControl
    {
        public event EventHandler mediaStop = (sender, e) => { };
        private MediaPlayer? player;
        public MediaController()
        {
            InitializeComponent();
        }
        public void setMedia(MediaPlayer player)
        {
            this.player = player;
            var mediaElement = player.media;
            if (mediaElement != null)
            {
                DispatcherTimer timer = new();
                timer.Interval = TimeSpan.FromSeconds(0.1);
                timer.Tick += timer_Tick;
                timer.Start();
                mediaElement.MediaEnded += mediaEnded;
                mediaElement.MediaFailed += mediaFailed;
                play();

            }
        }
        private void timer_Tick(object? sender, EventArgs e)
        {
            var mediaElement = player?.media;
            if (mediaElement != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                double nowSec = mediaElement.Position.TotalSeconds;
                double totalSec = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                progressBar.Value = nowSec * 100 / totalSec;


            }
        }


        public void play()
        {
            if (player != null)
            {
                player.Play();
                playButton.Visibility = Visibility.Hidden;
                pauseButton.Visibility = Visibility.Visible;
            }
        }
        public void pause()
        {
            if (player != null)
            {
                player.Pause();
                playButton.Visibility = Visibility.Visible;
                pauseButton.Visibility = Visibility.Hidden;
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            play();
            e.Handled = true;
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            pause();
            e.Handled = true;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaStop?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }

        private void mediaEnded(object sender, RoutedEventArgs e)
        {
            var mediaElement = player?.media;
            if (mediaElement != null)
            {
                mediaElement.Position = TimeSpan.FromSeconds(0);
                play();
            }
        }
        private void mediaFailed(object? sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Failed");
        }


        private void gridProgress_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mediaElement = player?.media;
            if (mediaElement != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                var totalSec = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                var pt = e.GetPosition((UIElement)sender);
                var positiion = (pt.X - progressBar.Margin.Left) / progressBar.ActualWidth * totalSec;
                mediaElement.Position = TimeSpan.FromSeconds(Math.Max(positiion, 0));
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
