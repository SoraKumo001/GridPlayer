using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GridPlayer
{
    public class WindowStatus : INotifyPropertyChanged
    {
        private double left = 0;
        private double top = 0;
        private double width = 640;
        private double height = 480;
        public double Left { get { return left; } set { if (left != value) { left = value; OnPropertyChanged(); } } }
        public double Top { get { return top; } set { if (top != value) { top = value; OnPropertyChanged(); } } }
        public double Width { get { return width; } set { if (width != value) { width = value; OnPropertyChanged(); } } }
        public double Height { get { return height; } set { if (height != value) { height = value; OnPropertyChanged(); } } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string info = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
