using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GridPlayer
{
    public class SkiaAnimationEngine : IMediaEngine
    {
        private readonly Image _skiaImage;
        private readonly string _path;
        private readonly int _decodePixelWidth;
        private readonly CancellationTokenSource _animationCts;
        private WriteableBitmap? _skiaBitmap;
        private bool _isPlaying;
        private bool _hasEnded;

        public event EventHandler? MediaOpened;
        public event EventHandler? MediaEnded;
        public event EventHandler<Exception>? MediaFailed;

        public SkiaAnimationEngine(Image skiaImage, string path, int decodePixelWidth)
        {
            _skiaImage = skiaImage;
            _path = path;
            _decodePixelWidth = decodePixelWidth;
            _animationCts = new CancellationTokenSource();
            _isPlaying = true;

            StartAnimation(_animationCts.Token);
        }

        private void StartAnimation(CancellationToken token)
        {
            Task.Run(async () =>
            {
                try
                {
                    using var stream = File.OpenRead(_path);
                    using var codec = SKCodec.Create(stream);
                    if (codec == null)
                    {
                        throw new InvalidDataException("Failed to create SKCodec. Invalid image format?");
                    }

                    var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888);

                    // Resize if needed
                    if (_decodePixelWidth > 0 && info.Width > _decodePixelWidth)
                    {
                        float ratio = (float)_decodePixelWidth / info.Width;
                        info = new SKImageInfo(_decodePixelWidth, (int)(info.Height * ratio), SKColorType.Bgra8888);
                    }

                    await _skiaImage.Dispatcher.InvokeAsync(() =>
                    {
                        _skiaBitmap = new WriteableBitmap(info.Width, info.Height, 96, 96, PixelFormats.Bgra32, null);
                        _skiaImage.Source = _skiaBitmap;
                        RenderOptions.SetBitmapScalingMode(_skiaImage, BitmapScalingMode.LowQuality);
                        MediaOpened?.Invoke(this, EventArgs.Empty);
                    });

                    var frameCount = codec.FrameCount;
                    var frameInfo = codec.FrameInfo;
                    using var bitmap = new SKBitmap(info);

                    while (!token.IsCancellationRequested)
                    {
                        for (int i = 0; i < frameCount; i++)
                        {
                            if (token.IsCancellationRequested) break;

                            while (!_isPlaying && !token.IsCancellationRequested)
                            {
                                await Task.Delay(100);
                            }
                            if (token.IsCancellationRequested) break;

                            var duration = frameInfo[i].Duration;

                            // Decode frame
                            var opts = new SKCodecOptions(i);
                            codec.GetPixels(info, bitmap.GetPixels(), opts);

                            // Update UI
                            await _skiaImage.Dispatcher.InvokeAsync(() =>
                            {
                                _skiaBitmap?.WritePixels(
                                    new Int32Rect(0, 0, info.Width, info.Height),
                                    bitmap.GetPixels(),
                                    info.RowBytes * info.Height,
                                    info.RowBytes);
                            });

                            await Task.Delay(duration > 0 ? duration : 100);
                        }
                        if (frameCount <= 1)
                        {
                            _hasEnded = true;
                            MediaEnded?.Invoke(this, EventArgs.Empty);
                            break; // Static image
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Skia Animation Error: {ex.Message}");
                    MediaFailed?.Invoke(this, ex);
                }
            }, token);
        }

        public void Play()
        {
            _isPlaying = true;
        }

        public void Pause()
        {
            _isPlaying = false;
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        public double Position
        {
            get => 0;
            set { }
        }

        public double Duration => 0;

        public bool HasDuration => false;

        public bool IsPlaying => _isPlaying;

        public bool HasEnded => _hasEnded;

        public double NaturalWidth => _skiaBitmap?.Width ?? 0;

        public double NaturalHeight => _skiaBitmap?.Height ?? 0;

        public string Path => _path;

        public void Dispose()
        {
            _animationCts.Cancel();
            _animationCts.Dispose();
            _isPlaying = false;
        }
    }
}
