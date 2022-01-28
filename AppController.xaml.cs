using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace GridPlayer
{
    /// <summary>
    /// AppController.xaml の相互作用ロジック
    /// </summary>
    public partial class AppController : UserControl
    {

        public event AppEventHandler controlEvents = (sender, e) => { };
        public bool IsVolume { get { return volumeController.IsVolume; } set { volumeController.IsVolume = value; } }
        public double Volume { get { return volumeController.Volume; } set { volumeController.Volume = value; } }
        public AppController()
        {
            InitializeComponent();
        }

        private void clearMedia_Click(object sender, RoutedEventArgs e)
        {
            controlEvents.Invoke(this, new AppEventArgs { type = "clear" });
        }
        public void active()
        {
            Storyboard storyboard = (Storyboard)this.appController.Resources["fadeStoryboard"];
            storyboard.Begin();
        }

        private void appController_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            Storyboard storyboard = (Storyboard)this.appController.Resources["fadeStoryboard"];
            storyboard.Stop();
            appController.Opacity = 1;
        }

        private void appController_MouseLeave(object sender, MouseEventArgs e)
        {
            active();
        }

        private void VolumeController_controlEvents(object sender, AppEventArgs e)
        {
            controlEvents.Invoke(sender, e);
        }
    }
}
