using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using SkiaSharp;
using System;
using System.Threading.Tasks;

namespace grid_image_viewer.Controls
{
    public sealed partial class ToneAdjustmentOverlay : UserControl
    {
        private readonly MainWindow _window;
        private readonly string _sourcePath;
        private readonly SKBitmap? _baseBmp;
        private readonly DispatcherTimer _updateTimer;

        public ToneAdjustmentOverlay(MainWindow window, string sourcePath, SKBitmap? baseBmp)
        {
            this.InitializeComponent();
            _window = window;
            _sourcePath = sourcePath;
            _baseBmp = baseBmp;

            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _updateTimer.Tick += OnUpdateTimerTick;
            
            // Initial focus
            BtnClose.Focus(FocusState.Programmatic);
        }

        private async void OnUpdateTimerTick(object? sender, object e)
        {
            _updateTimer.Stop();
            float b = (float)SliderBrightness.Value;
            float c = (float)SliderContrast.Value;
            float sVal = (float)SliderSaturation.Value;

            await Task.Run(() =>
            {
                try
                {
                    var newBmp = ImageProcessor.ApplyToneAdjustment(_sourcePath, b, c, sVal, _baseBmp);
                    if (newBmp != null)
                    {
                        _window.DispatcherQueue.TryEnqueue(() =>
                        {
                            if (_window.ViewerManager == null) return;
                            _window.ViewerManager.AddPendingEdit(_sourcePath, newBmp);
                            _window.ViewerManager.StopAnimation();
                            
                            foreach (var img in _window.ViewerManager.PageImages) img.Source = null;
                            for (int pi = 0; pi < _window.ViewerManager.Pages.Length; pi++)
                            {
                                if (_window.ViewerManager.Pages[pi].CurrentFilePath == _sourcePath)
                                    _window.ViewerManager.Pages[pi].EditedBitmap = null;
                            }
                            _ = _window.UpdateDisplayAsync();
                        });
                    }
                }
                catch { }
            });
        }

        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Text values are handled by x:Bind or manual update if x:Bind is tricky
            // For simplicity, let's just trigger the timer
            _updateTimer?.Stop();
            _updateTimer?.Start();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            SliderBrightness.Value = 0;
            SliderContrast.Value = 1.0;
            SliderSaturation.Value = 1.0;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Overlay_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void Close()
        {
            var parent = this.Parent as Panel;
            parent?.Children.Remove(this);
            _window.IsDialogOpen = false;
            _baseBmp?.Dispose();
        }
    }
}
