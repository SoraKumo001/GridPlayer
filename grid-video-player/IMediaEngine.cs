using System;

namespace GridPlayer
{
    public interface IMediaEngine : IDisposable
    {
        void Play();
        void Pause();
        void Stop();
        void Restart();
        double Position { get; set; }
        double Duration { get; }
        bool HasDuration { get; }
        bool IsPlaying { get; }
        bool HasEnded { get; }
        double NaturalWidth { get; }
        double NaturalHeight { get; }
        string Path { get; }

        event EventHandler? MediaOpened;
        event EventHandler? MediaEnded;
        event EventHandler<Exception>? MediaFailed;
    }
}
