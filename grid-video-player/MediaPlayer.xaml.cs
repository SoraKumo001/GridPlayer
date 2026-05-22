using System;
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
        public event EventHandler? mediaStop;
        public event EventHandler? mediaOpen;

        public event EventHandler? MediaOpened;
        public event EventHandler? MediaEnded;
        public event EventHandler<Exception>? MediaFailed;

        public DispatcherTimer timer = new();
        private IMediaEngine? _mediaEngine;
        private readonly Settings _settings;

        public MediaPlayer()
        {
            InitializeComponent();
            _settings = ((App)Application.Current).settings;
            DataContext = _settings.appStatus;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            mediaController.Visibility = Visibility.Hidden;
            filenameText.Visibility = Visibility.Hidden;
        }

        public void play(string path)
        {
            _mediaEngine?.Dispose();
            _mediaEngine = null;

            filenameText.Text = System.IO.Path.GetFileName(path);

            if (path.ToLower().EndsWith(".webp") || path.ToLower().EndsWith(".gif"))
            {
                mediaElement.Visibility = Visibility.Collapsed;
                ffmeElement.Visibility = Visibility.Collapsed;
                animatedImage.Visibility = Visibility.Collapsed;
                skiaImage.Visibility = Visibility.Visible;
                _mediaEngine = new SkiaAnimationEngine(skiaImage, path, _settings.appStatus.DecodePixelWidth);
            }
            else if (path.ToLower().EndsWith(".webm"))
            {
                mediaElement.Visibility = Visibility.Collapsed;
                skiaImage.Visibility = Visibility.Collapsed;
                animatedImage.Visibility = Visibility.Collapsed;
                ffmeElement.Visibility = Visibility.Visible;
                _mediaEngine = new FfmeMediaEngine(ffmeElement, path);
            }
            else
            {
                mediaElement.Visibility = Visibility.Visible;
                ffmeElement.Visibility = Visibility.Collapsed;
                animatedImage.Visibility = Visibility.Collapsed;
                skiaImage.Visibility = Visibility.Collapsed;
                _mediaEngine = new StandardMediaEngine(mediaElement, path);
            }

            _mediaEngine.MediaOpened += (s, e) =>
            {
                mediaOpen?.Invoke(this, EventArgs.Empty);
                MediaOpened?.Invoke(this, EventArgs.Empty);
            };
            _mediaEngine.MediaEnded += (s, e) => MediaEnded?.Invoke(this, EventArgs.Empty);
            _mediaEngine.MediaFailed += (s, ex) => MediaFailed?.Invoke(this, ex);

            _mediaEngine.Play();

            mediaController.setMedia(this);
            mediaController.mediaStop += _mediaStop;
        }

        public void Play() => _mediaEngine?.Play();
        public void Pause() => _mediaEngine?.Pause();
        public bool IsPlaying => _mediaEngine?.IsPlaying ?? false;
        public bool HasEnded => _mediaEngine?.HasEnded ?? false;

        public void Restart()
        {
            _mediaEngine?.Restart();
        }

        private void _mediaStop(object? sender, EventArgs e)
        {
            _mediaEngine?.Dispose();
            _mediaEngine = null;
            mediaStop?.Invoke(this, EventArgs.Empty);
        }

        public string Path => _mediaEngine?.Path ?? "";

        public double Position
        {
            get => _mediaEngine?.Position ?? 0;
            set { if (_mediaEngine != null) _mediaEngine.Position = value; }
        }

        public double Duration => _mediaEngine?.Duration ?? 0;
        public bool HasDuration => _mediaEngine?.HasDuration ?? false;

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            mediaController.Visibility = Visibility.Visible;
            filenameText.Visibility = Visibility.Visible;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Tick += (object? sender, EventArgs e) =>
            {
                mediaController.Visibility = Visibility.Hidden;
                filenameText.Visibility = Visibility.Hidden;
                timer.Stop();
            };
            this.Unloaded += (s, args) => _mediaEngine?.Dispose();
        }

        public double NaturalWidth => _mediaEngine?.NaturalWidth ?? 0;
        public double NaturalHeight => _mediaEngine?.NaturalHeight ?? 0;
    }
}

