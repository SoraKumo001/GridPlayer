using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GridPlayer
{
    /// <summary>
    /// VolumeController.xaml の相互作用ロジック
    /// </summary>
    public partial class VolumeController : UserControl
    {
        public event AppEventHandler controlEvents = (sender, e) => { };
        public bool IsVolume
        {
            set
            {
                if (value)
                {
                    volumeOn.Visibility = Visibility.Visible;
                    volumeOff.Visibility = Visibility.Hidden;
                    controlEvents.Invoke(this, new AppEventArgs { type = "volumeOn" });
                }
                else
                {
                    volumeOn.Visibility = Visibility.Hidden;
                    volumeOff.Visibility = Visibility.Visible;
                    controlEvents.Invoke(this, new AppEventArgs { type = "volumeOff" });
                }
            }
            get
            {
                return volumeOn.Visibility == Visibility.Visible;
            }
        }
        public double Volume
        {
            get { return progressBar.Value; }
            set { progressBar.Value = value; }
        }
        public VolumeController()
        {
            InitializeComponent();
        }

        private void volumeOn_Click(object sender, RoutedEventArgs e)
        {
            IsVolume = false;
        }

        private void volumeOff_Click(object sender, RoutedEventArgs e)
        {
            IsVolume = true;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

            var pt = e.GetPosition((UIElement)sender);
            var positiion = (pt.X - progressBar.Margin.Left) / progressBar.ActualWidth * 100;
            progressBar.Value = Math.Clamp(positiion, 0, 100);
            controlEvents.Invoke(this, new AppEventArgs { type = "volume", value = progressBar.Value });

        }
    }
}
