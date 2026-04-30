using System;
using System.Collections.Generic;
using System.Globalization;
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
