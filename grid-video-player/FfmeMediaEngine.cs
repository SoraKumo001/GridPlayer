using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GridPlayer
{
    public class FfmeMediaEngine : IMediaEngine
    {
        private readonly Unosquare.FFME.MediaElement _ffmeElement;
        private readonly string _path;
        private readonly CancellationTokenSource _openCts;
        private bool _isPlaying;
        private bool _isReady;
        private bool _pendingPlay;
        private bool _isRestarting;

        public event EventHandler? MediaOpened;
        public event EventHandler? MediaEnded;
        public event EventHandler<Exception>? MediaFailed;

        public FfmeMediaEngine(Unosquare.FFME.MediaElement ffmeElement, string path)
        {
            _ffmeElement = ffmeElement;
            _path = path;
            _openCts = new CancellationTokenSource();

            _ffmeElement.MediaOpened += OnMediaOpened;
            _ffmeElement.MediaEnded += OnMediaEnded;
            _ffmeElement.MediaFailed += OnMediaFailed;

            _pendingPlay = true;
            _ = OpenAsync(_openCts.Token);
        }

        private async Task OpenAsync(CancellationToken token)
        {
            try
            {
                await _ffmeElement.Open(new Uri(_path));
                if (token.IsCancellationRequested) return;

                _isReady = true;
                if (_pendingPlay)
                {
                    await _ffmeElement.Play();
                    _isPlaying = true;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFME Open Exception: {ex.Message}");
                MediaFailed?.Invoke(this, ex);
            }
        }

        private void OnMediaOpened(object? sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            MediaOpened?.Invoke(this, EventArgs.Empty);
        }

        private void OnMediaEnded(object? sender, EventArgs e)
        {
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        private void OnMediaFailed(object? sender, Unosquare.FFME.Common.MediaFailedEventArgs e)
        {
            MediaFailed?.Invoke(this, e.ErrorException ?? new Exception("FFME playback error"));
        }

        public void Play()
        {
            _pendingPlay = true;
            if (_isReady)
            {
                _ = PlayAsync();
            }
        }

        private async Task PlayAsync()
        {
            try
            {
                await _ffmeElement.Play();
                _isPlaying = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFME Play Exception: {ex.Message}");
            }
        }

        public void Pause()
        {
            _pendingPlay = false;
            if (_isReady)
            {
                _ = PauseAsync();
            }
        }

        private async Task PauseAsync()
        {
            try
            {
                if (_isReady)
                {
                    await _ffmeElement.Pause();
                }
                _isPlaying = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFME Pause Exception: {ex.Message}");
            }
        }

        public void Stop()
        {
            _pendingPlay = false;
            _ = StopAsync();
        }

        private async Task StopAsync()
        {
            try
            {
                _isPlaying = false;
                if (_isReady)
                {
                    await _ffmeElement.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFME Stop Exception: {ex.Message}");
            }
        }

        public double Position
        {
            get => _ffmeElement.Position.TotalSeconds;
            set
            {
                if (_isReady)
                {
                    _ffmeElement.Position = TimeSpan.FromSeconds(value);
                }
            }
        }

        public double Duration => _ffmeElement.NaturalDuration.HasValue ? _ffmeElement.NaturalDuration.Value.TotalSeconds : 0;

        public bool HasDuration => _ffmeElement.NaturalDuration.HasValue;

        public bool IsPlaying => _isPlaying;

        public bool HasEnded => _ffmeElement.HasMediaEnded;

        public double NaturalWidth => _ffmeElement.NaturalVideoWidth;

        public double NaturalHeight => _ffmeElement.NaturalVideoHeight;

        public string Path => _path;

        public void Restart()
        {
            if (_isReady && !_isRestarting)
            {
                _ = RestartAsync();
            }
        }

        private async Task RestartAsync()
        {
            _isRestarting = true;
            try
            {
                _isPlaying = false;
                await _ffmeElement.Stop();
                await _ffmeElement.Seek(TimeSpan.Zero);
                await _ffmeElement.Play();
                _isPlaying = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFME Restart Exception: {ex.Message}");
            }
            finally
            {
                _isRestarting = false;
            }
        }

        public void Dispose()
        {
            _openCts.Cancel();
            _openCts.Dispose();

            _ffmeElement.MediaOpened -= OnMediaOpened;
            _ffmeElement.MediaEnded -= OnMediaEnded;
            _ffmeElement.MediaFailed -= OnMediaFailed;

            _isPlaying = false;
            _isReady = false;

            _ = _ffmeElement.Close();
        }
    }
}
