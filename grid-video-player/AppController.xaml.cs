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

        public AppController()
        {
            InitializeComponent();
        }

        private void clearMedia_Click(object sender, RoutedEventArgs e)
        {
            var settings = ((App)Application.Current).settings;
            settings.mediaList[0].mediaData.Clear();
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

        private void listMedia_Click(object sender, RoutedEventArgs e)
        {
            var w = new MediaListWindow();
            w.Owner = Window.GetWindow(this);
            w.Show();
        }
    }
}
