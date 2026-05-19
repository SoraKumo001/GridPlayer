using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using Unosquare.FFME;

namespace GridPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Settings settings = new();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        public App()
        {
            var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
            Library.FFmpegDirectory = ffmpegPath;
            SetDllDirectory(ffmpegPath);
            Environment.SetEnvironmentVariable(
                "PATH",
                ffmpegPath + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH"));

            // Check if DLLs exist
            if (Directory.Exists(ffmpegPath))
            {
                var files = Directory.GetFiles(ffmpegPath, "*.dll");
                Debug.WriteLine($"FFmpeg DLLs found in {ffmpegPath}: {files.Length} files.");
                try
                {
                    Library.LoadFFmpeg();
                    Debug.WriteLine($"FFmpeg loaded: {Library.FFmpegVersionInfo}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"FFmpeg load failed: {ex}");
                }
            }
            else
            {
                Debug.WriteLine($"FFmpeg directory NOT FOUND: {ffmpegPath}");
            }

            settings.load();
        }

        override protected void OnExit(ExitEventArgs e)
        {
            settings.save();
        }



    }
    public class AppEventArgs
    {
        public string type = "";
        public double value = 0;
    }
    public delegate void AppEventHandler(object? sender, AppEventArgs e);

    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, True);
        }
    }
    public class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }
}
