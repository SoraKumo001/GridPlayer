using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;

namespace GridPlayer
{
    public class AppStatus : INotifyPropertyChanged
    {
        bool isMute = true;
        double volume = 1.0;
        public bool IsMute { get { return isMute; } set { if (isMute != value) { isMute = value; OnPropertyChanged(); } } }
        public double Volume { get { return volume; } set { if (volume != value) { volume = value; OnPropertyChanged(); } } }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }

}
