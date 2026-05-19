using System;
using System.Windows;
using System.Windows.Controls;

namespace GridPlayer
{
    /// <summary>
    /// VolumeController.xaml の相互作用ロジック
    /// </summary>
    public partial class VolumeController : UserControl
    {
        public VolumeController()
        {
            InitializeComponent();
            try
            {
                var settings = ((App)Application.Current).settings;
                DataContext = settings.appStatus;
            }
            catch (Exception) { }
        }

        private void volumeOn_Click(object sender, RoutedEventArgs e)
        {
            var settings = ((App)Application.Current).settings;
            settings.appStatus.IsMute = true;
        }

        private void volumeOff_Click(object sender, RoutedEventArgs e)
        {
            var settings = ((App)Application.Current).settings;
            settings.appStatus.IsMute = false;
        }

    }
}
