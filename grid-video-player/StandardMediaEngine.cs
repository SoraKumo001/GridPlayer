using System;
using System.Windows;
using System.Windows.Controls;

namespace GridPlayer
{
    public class StandardMediaEngine : IMediaEngine
    {
        private readonly MediaElement _mediaElement;
        private bool _isPlaying;
        private readonly string _path;

        public event EventHandler? MediaOpened;
        public event EventHandler? MediaEnded;
        public event EventHandler<Exception>? MediaFailed;

        public StandardMediaEngine(MediaElement mediaElement, string path)
        {
            _mediaElement = mediaElement;
            _path = path;

            _mediaElement.LoadedBehavior = MediaState.Manual;
            _mediaElement.Source = new Uri(path);

            _mediaElement.MediaOpened += OnMediaOpened;
            _mediaElement.MediaEnded += OnMediaEnded;
            _mediaElement.MediaFailed += OnMediaFailed;
        }

        private void OnMediaOpened(object sender, RoutedEventArgs e)
        {
            MediaOpened?.Invoke(this, EventArgs.Empty);
        }

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        private void OnMediaFailed(object? sender, ExceptionRoutedEventArgs e)
        {
            MediaFailed?.Invoke(this, e.ErrorException ?? new Exception("Unknown media playback error"));
        }

        public void Play()
        {
            _mediaElement.Play();
            _isPlaying = true;
        }

        public void Pause()
        {
            _mediaElement.Pause();
            _isPlaying = false;
        }

        public void Stop()
        {
            _mediaElement.Stop();
            _isPlaying = false;
        }

        public void Restart()
        {
            _mediaElement.Stop();
            _mediaElement.Position = TimeSpan.Zero;
            _mediaElement.Play();
            _isPlaying = true;
        }

        public double Position
        {
            get => _mediaElement.Position.TotalSeconds;
            set => _mediaElement.Position = TimeSpan.FromSeconds(value);
        }

        public double Duration => _mediaElement.NaturalDuration.HasTimeSpan ? _mediaElement.NaturalDuration.TimeSpan.TotalSeconds : 0;

        public bool HasDuration => _mediaElement.NaturalDuration.HasTimeSpan;

        public bool IsPlaying => _isPlaying;

        public bool HasEnded => _mediaElement.NaturalDuration.HasTimeSpan && _mediaElement.Position >= _mediaElement.NaturalDuration.TimeSpan;

        public double NaturalWidth => _mediaElement.NaturalVideoWidth;

        public double NaturalHeight => _mediaElement.NaturalVideoHeight;

        public string Path => _path;

        public void Dispose()
        {
            _mediaElement.MediaOpened -= OnMediaOpened;
            _mediaElement.MediaEnded -= OnMediaEnded;
            _mediaElement.MediaFailed -= OnMediaFailed;
            _mediaElement.Source = null;
            _isPlaying = false;
        }
    }
}
