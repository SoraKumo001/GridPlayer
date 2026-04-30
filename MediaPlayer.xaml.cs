using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AnimatedImage.Wpf;

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
        bool isPlaying = false;
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
            if (path.ToLower().EndsWith(".webp") || path.ToLower().EndsWith(".gif"))
            {
                mediaElement.Visibility = Visibility.Collapsed;
                animatedImage.Visibility = Visibility.Visible;
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ImageBehavior.SetAnimatedSource(animatedImage, bitmap);
                Dispatcher.BeginInvoke(new Action(() => mediaOpen.Invoke(this, EventArgs.Empty)), DispatcherPriority.Background);
            }
            else
            {
                mediaElement.Visibility = Visibility.Visible;
                animatedImage.Visibility = Visibility.Collapsed;
                mediaElement.LoadedBehavior = MediaState.Manual;
                mediaElement.Source = new Uri(path);
            }
            mediaController.setMedia(this);
            mediaController.mediaStop += _mediaStop;
            Play();
        }
        public void Play()
        {
            isPlaying = true;
            if (animatedImage.Visibility == Visibility.Visible)
            {
                var controller = ImageBehavior.GetAnimationController(animatedImage);
                controller?.Play();
            }
            else
            {
                mediaElement.Play();
            }
        }
        public void Pause()
        {
            isPlaying = false;
            if (animatedImage.Visibility == Visibility.Visible)
            {
                var controller = ImageBehavior.GetAnimationController(animatedImage);
                controller?.Pause();
            }
            else
            {
                mediaElement.Pause();
            }
        }
        public bool IsPlaying { get { return isPlaying; } }
        private void _mediaStop(object? sender, EventArgs e)
        {
            mediaStop.Invoke(this, EventArgs.Empty);
            isPlaying = false;
        }
        public string Path
        {
            get
            {
                if (animatedImage.Visibility == Visibility.Visible)
                {
                    var source = ImageBehavior.GetAnimatedSource(animatedImage) as BitmapImage;
                    return source?.UriSource.LocalPath ?? "";
                }
                return mediaElement.Source?.LocalPath ?? "";
            }
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
            mediaOpen.Invoke(this, EventArgs.Empty);
        }
        public double NaturalWidth
        {
            get
            {
                if (animatedImage.Visibility == Visibility.Visible) return animatedImage.Source?.Width ?? 0;
                return mediaElement.NaturalVideoWidth;
            }
        }
        public double NaturalHeight
        {
            get
            {
                if (animatedImage.Visibility == Visibility.Visible) return animatedImage.Source?.Height ?? 0;
                return mediaElement.NaturalVideoHeight;
            }
        }
    }
}
