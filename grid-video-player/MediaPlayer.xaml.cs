using AnimatedImage.Wpf;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        bool isPlaying = false;
        Settings settings;
        private CancellationTokenSource? _animationCts;
        private CancellationTokenSource? _ffmeOpenCts;
        private WriteableBitmap? _skiaBitmap;
        private string _currentPath = "";
        private bool _ffmeReady;
        private bool _pendingFfmePlay;
        private bool _isRestartingFfme;

        public System.Windows.Controls.MediaElement media { get { return mediaElement; } }
        public MediaPlayer()
        {
            InitializeComponent();
            settings = ((App)Application.Current).settings;
            DataContext = settings.appStatus;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            mediaController.Visibility = Visibility.Hidden;
            filenameText.Visibility = Visibility.Hidden;
        }
        public void play(string path)
        {
            _currentPath = path;
            _animationCts?.Cancel();
            _ffmeOpenCts?.Cancel();
            _ffmeReady = false;
            _pendingFfmePlay = false;
            filenameText.Text = System.IO.Path.GetFileName(path);

            if (path.ToLower().EndsWith(".webp") || path.ToLower().EndsWith(".gif"))
            {
                mediaElement.Visibility = Visibility.Collapsed;
                ffmeElement.Visibility = Visibility.Collapsed;
                animatedImage.Visibility = Visibility.Collapsed;
                skiaImage.Visibility = Visibility.Visible;
                StartSkiaAnimation(path);
            }
            else if (path.ToLower().EndsWith(".webm"))
            {
                mediaElement.Visibility = Visibility.Collapsed;
                skiaImage.Visibility = Visibility.Collapsed;
                animatedImage.Visibility = Visibility.Collapsed;
                ffmeElement.Visibility = Visibility.Visible;
                _pendingFfmePlay = true;
                _ffmeOpenCts = new CancellationTokenSource();
                _ = OpenFfmeAsync(path, _ffmeOpenCts.Token);
            }
            else
            {
                mediaElement.Visibility = Visibility.Visible;
                ffmeElement.Visibility = Visibility.Collapsed;
                animatedImage.Visibility = Visibility.Collapsed;
                skiaImage.Visibility = Visibility.Collapsed;
                mediaElement.LoadedBehavior = MediaState.Manual;
                mediaElement.Source = new Uri(path);
                Play();
            }
            mediaController.setMedia(this);
            mediaController.mediaStop += _mediaStop;
        }

        private async Task OpenFfmeAsync(string path, CancellationToken token)
        {
            try
            {
                await ffmeElement.Open(new Uri(path));
                if (token.IsCancellationRequested || _currentPath != path) return;

                _ffmeReady = true;
                if (_pendingFfmePlay)
                {
                    await ffmeElement.Play();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFME Open Exception: {ex.Message}");
            }
        }

        private async void StartSkiaAnimation(string path)
        {
            _animationCts = new CancellationTokenSource();
            var token = _animationCts.Token;

            try
            {
                await Task.Run(async () =>
                {
                    using var stream = File.OpenRead(path);
                    using var codec = SKCodec.Create(stream);
                    if (codec == null) return;

                    var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888);

                    // Apply DecodePixelWidth if needed
                    if (settings.appStatus.DecodePixelWidth > 0 && info.Width > settings.appStatus.DecodePixelWidth)
                    {
                        float ratio = (float)settings.appStatus.DecodePixelWidth / info.Width;
                        info = new SKImageInfo(settings.appStatus.DecodePixelWidth, (int)(info.Height * ratio), SKColorType.Bgra8888);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        _skiaBitmap = new WriteableBitmap(info.Width, info.Height, 96, 96, PixelFormats.Bgra32, null);
                        skiaImage.Source = _skiaBitmap;
                        RenderOptions.SetBitmapScalingMode(skiaImage, BitmapScalingMode.LowQuality);
                        mediaOpen.Invoke(this, EventArgs.Empty);
                    });

                    var frameCount = codec.FrameCount;
                    var frameInfo = codec.FrameInfo;
                    using var bitmap = new SKBitmap(info);

                    while (!token.IsCancellationRequested)
                    {
                        for (int i = 0; i < frameCount; i++)
                        {
                            if (token.IsCancellationRequested) break;

                            while (!isPlaying && !token.IsCancellationRequested)
                            {
                                await Task.Delay(100);
                            }
                            if (token.IsCancellationRequested) break;

                            var duration = frameInfo[i].Duration;

                            // Decode frame
                            var opts = new SKCodecOptions(i);
                            codec.GetPixels(info, bitmap.GetPixels(), opts);

                            // Update UI
                            Dispatcher.Invoke(() =>
                            {
                                _skiaBitmap?.WritePixels(new Int32Rect(0, 0, info.Width, info.Height), bitmap.GetPixels(), info.RowBytes * info.Height, info.RowBytes);
                            });

                            await Task.Delay(duration > 0 ? duration : 100);
                        }
                        if (frameCount <= 1) break; // Static image
                    }
                }, token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"Skia Animation Error: {ex.Message}");
            }
        }

        public void Play()
        {
            isPlaying = true;
            if (skiaImage.Visibility == Visibility.Visible)
            {
                // Animation loop handles isPlaying
            }
            else if (ffmeElement.Visibility == Visibility.Visible)
            {
                _pendingFfmePlay = true;
                if (_ffmeReady)
                {
                    _ = PlayFfmeAsync();
                }
            }
            else if (animatedImage.Visibility == Visibility.Visible)
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
            if (skiaImage.Visibility == Visibility.Visible)
            {
                // Animation loop handles isPlaying
            }
            else if (ffmeElement.Visibility == Visibility.Visible)
            {
                _pendingFfmePlay = false;
                _ = PauseFfmeAsync();
            }
            else if (animatedImage.Visibility == Visibility.Visible)
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
        public bool HasEnded
        {
            get
            {
                if (ffmeElement.Visibility == Visibility.Visible) return ffmeElement.HasMediaEnded;
                return mediaElement.NaturalDuration.HasTimeSpan
                    && mediaElement.Position >= mediaElement.NaturalDuration.TimeSpan;
            }
        }

        public void Restart()
        {
            isPlaying = true;
            if (ffmeElement.Visibility == Visibility.Visible)
            {
                _pendingFfmePlay = true;
                _ = RestartFfmeAsync();
            }
            else
            {
                Position = 0;
                Play();
            }
        }

        private async Task RestartFfmeAsync()
        {
            if (_isRestartingFfme) return;

            try
            {
                if (!_ffmeReady) return;

                _isRestartingFfme = true;
                await ffmeElement.Stop();
                await ffmeElement.Seek(TimeSpan.Zero);
                await ffmeElement.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFME Restart Exception: {ex.Message}");
            }
            finally
            {
                _isRestartingFfme = false;
            }
        }

        private async Task PlayFfmeAsync()
        {
            try
            {
                await ffmeElement.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFME Play Exception: {ex.Message}");
            }
        }

        private async Task PauseFfmeAsync()
        {
            try
            {
                if (_ffmeReady) await ffmeElement.Pause();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFME Pause Exception: {ex.Message}");
            }
        }

        private void _mediaStop(object? sender, EventArgs e)
        {
            _animationCts?.Cancel();
            _ffmeOpenCts?.Cancel();
            _pendingFfmePlay = false;
            _ffmeReady = false;
            _isRestartingFfme = false;
            if (ffmeElement.Visibility == Visibility.Visible) _ = ffmeElement.Close();
            mediaStop.Invoke(this, EventArgs.Empty);
            isPlaying = false;
        }
        public string Path
        {
            get
            {
                if (skiaImage.Visibility == Visibility.Visible)
                {
                    return _currentPath;
                }
                if (ffmeElement.Visibility == Visibility.Visible)
                {
                    return ffmeElement.Source?.LocalPath ?? "";
                }
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
            get
            {
                if (ffmeElement.Visibility == Visibility.Visible) return ffmeElement.Position.TotalSeconds;
                return mediaElement.Position.TotalSeconds;
            }
            set
            {
                if (ffmeElement.Visibility == Visibility.Visible) ffmeElement.Position = TimeSpan.FromSeconds(value);
                else mediaElement.Position = TimeSpan.FromSeconds(value);
            }

        }


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
            this.Unloaded += (s, args) => _animationCts?.Cancel();
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            mediaOpen.Invoke(this, EventArgs.Empty);
        }
        private void ffmeElement_MediaOpened(object? sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            mediaOpen.Invoke(this, EventArgs.Empty);
        }
        public double NaturalWidth
        {
            get
            {
                if (skiaImage.Visibility == Visibility.Visible && _skiaBitmap != null) return _skiaBitmap.Width;
                if (ffmeElement.Visibility == Visibility.Visible) return ffmeElement.NaturalVideoWidth;
                if (animatedImage.Visibility == Visibility.Visible) return animatedImage.Source?.Width ?? 0;
                return mediaElement.NaturalVideoWidth;
            }
        }
        public double NaturalHeight
        {
            get
            {
                if (skiaImage.Visibility == Visibility.Visible && _skiaBitmap != null) return _skiaBitmap.Height;
                if (ffmeElement.Visibility == Visibility.Visible) return ffmeElement.NaturalVideoHeight;
                if (animatedImage.Visibility == Visibility.Visible) return animatedImage.Source?.Height ?? 0;
                return mediaElement.NaturalVideoHeight;
            }
        }
    }
}
