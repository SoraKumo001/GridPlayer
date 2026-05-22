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
            if (this.player != null)
            {
                this.player.MediaEnded -= mediaEnded;
                this.player.MediaFailed -= mediaFailed;
            }

            this.player = player;
            this.player.MediaEnded += mediaEnded;
            this.player.MediaFailed += mediaFailed;

            DispatcherTimer timer = new();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += timer_Tick;
            timer.Start();

            play();
        }
        private void timer_Tick(object? sender, EventArgs e)
        {
            if (player == null) return;

            if (player.HasDuration && player.Duration > 0)
            {
                var nowSec = player.Position;
                var totalSec = player.Duration;
                var progress = nowSec / totalSec;
                if (!double.IsNaN(progress) && !double.IsInfinity(progress))
                {
                    seekSlider.Value = progress;
                }
                timeText.Text = string.Format("{0:mm\\:ss} / {1:mm\\:ss}", TimeSpan.FromSeconds(nowSec), TimeSpan.FromSeconds(totalSec));
            }

            if (player.IsPlaying && player.HasEnded)
            {
                onMediaEnded();
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

        private void mediaEnded(object? sender, EventArgs e)
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
        private void mediaFailed(object? sender, Exception e)
        {
            Debug.WriteLine($"Media failed: {e.Message}");
        }

        private void seekSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (seekSlider.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed && player != null)
            {
                if (player.HasDuration)
                {
                    player.Position = e.NewValue * player.Duration;
                }
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
