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

                player.ffmeElement.MediaEnded += ffme_MediaEnded;
                player.ffmeElement.MediaFailed += ffme_MediaFailed;

                play();

            }
        }
        private void timer_Tick(object? sender, EventArgs e)
        {
            if (player == null) return;

            bool hasDuration = false;
            double nowSec = 0;
            double totalSec = 0;
            TimeSpan position = TimeSpan.Zero;
            TimeSpan duration = TimeSpan.Zero;

            if (player.ffmeElement.Visibility == Visibility.Visible)
            {
                if (player.ffmeElement.NaturalDuration.HasValue)
                {
                    hasDuration = true;
                    nowSec = player.ffmeElement.Position.TotalSeconds;
                    totalSec = player.ffmeElement.NaturalDuration.Value.TotalSeconds;
                    position = player.ffmeElement.Position;
                    duration = player.ffmeElement.NaturalDuration.Value;
                }

                if (player.IsPlaying && player.HasEnded)
                {
                    onMediaEnded();
                    return;
                }
            }
            else
            {
                var mediaElement = player.media;
                if (mediaElement != null && mediaElement.NaturalDuration.HasTimeSpan)
                {
                    hasDuration = true;
                    nowSec = mediaElement.Position.TotalSeconds;
                    totalSec = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                    position = mediaElement.Position;
                    duration = mediaElement.NaturalDuration.TimeSpan;
                }
            }

            if (hasDuration && totalSec > 0)
            {
                var progress = nowSec / totalSec;
                if (!double.IsNaN(progress) && !double.IsInfinity(progress))
                {
                    seekSlider.Value = progress;
                }
                timeText.Text = string.Format("{0:mm\\:ss} / {1:mm\\:ss}", position, duration);
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
            onMediaEnded();
        }
        private void ffme_MediaEnded(object? sender, EventArgs e)
        {
            onMediaEnded();
        }
        private void onMediaEnded()
        {
            if (player != null)
            {
                player.Restart();
                playButton.Visibility = Visibility.Hidden;
                pauseButton.Visibility = Visibility.Visible;
            }
        }
        private void mediaFailed(object? sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Failed");
        }
        private void ffme_MediaFailed(object? sender, Unosquare.FFME.Common.MediaFailedEventArgs e)
        {
            Debug.WriteLine($"FFME Failed: {e.ErrorException}");
        }


        private void seekSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (seekSlider.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed && player != null)
            {
                double totalSec = 0;
                bool hasDuration = false;

                if (player.ffmeElement.Visibility == Visibility.Visible)
                {
                    if (player.ffmeElement.NaturalDuration.HasValue)
                    {
                        hasDuration = true;
                        totalSec = player.ffmeElement.NaturalDuration.Value.TotalSeconds;
                    }
                }
                else
                {
                    var mediaElement = player.media;
                    if (mediaElement != null && mediaElement.NaturalDuration.HasTimeSpan)
                    {
                        hasDuration = true;
                        totalSec = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                    }
                }

                if (hasDuration)
                {
                    player.Position = e.NewValue * totalSec;
                }
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
