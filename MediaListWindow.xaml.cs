using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GridPlayer
{
    class NameList
    {
        public int index { get; set; } = 0;
        public string name { get; set; } = "";
    }

    /// <summary>
    /// MediaListWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MediaListWindow : Window
    {
        public delegate void LocalEventHandler(object? sender);
        public event LocalEventHandler update = (sender) => { };
        Settings settings = ((App)Application.Current).settings;
        DispatcherTimer? timer;
        public MediaListWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mediaNameList.ItemsSource = settings.mediaList;
            mediaNameList.SelectedIndex = 0;
        }

        private void mediaNameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mediaPathList.ItemsSource = null;
            if (mediaNameList.SelectedIndex >= 0)
            {
                mediaPathList.ItemsSource = settings.mediaList[mediaNameList.SelectedIndex].mediaData;

            }
        }

        private void pathCopy_Click(object sender, RoutedEventArgs e)
        {
            settings.mediaList.Add(new MediaDataList()
            {
                name = "medias",
                mediaData = new ObservableCollection<MediaData>(settings.mediaList[0].mediaData)
            });
            mediaNameList.SelectedIndex = settings.mediaList.Count - 1;


        }

        private void pathDelete_Click(object sender, RoutedEventArgs e)
        {

            var item = (MediaData)mediaPathList.SelectedItem;
            if (item != null)
            {
                settings.mediaList[mediaNameList.SelectedIndex].mediaData.Remove(item);
            }
        }
        private void nameDelete_Click(object sender, RoutedEventArgs e)
        {

            var item = (MediaDataList)mediaNameList.SelectedItem;
            if (item != null && settings.mediaList[0] != item)
            {
                settings.mediaList.Remove(item);
            }

        }

        private void mediaNameList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = (ListViewItem)sender;
            MediaDataList list = (MediaDataList)item.DataContext;
            settings.mediaList[0].mediaData.Clear();
            foreach (var d in list.mediaData)
                settings.mediaList[0].mediaData.Add(d);

            mediaNameList.SelectedIndex = 0;
        }

        private void mediaNameList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var index = mediaNameList.SelectedIndex;
            if (index >= 0)
            {
                var item = mediaNameList.Items[index];


            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timer?.Stop();
            var index = mediaNameList.SelectedIndex;
            if (index >= 0 && e.ClickCount == 1)
            {
                TextBox txt = (TextBox)((Grid)((TextBlock)sender).Parent).Children[1];
                int i;
                for (i = 0; i < settings.mediaList.Count && settings.mediaList[i] != txt.DataContext; i++) ;
                if (i == index)
                {
                    timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.2) };
                    timer.Tick += (e, args) =>
                    {
                        timer.Stop();
                        txt.Visibility = Visibility.Visible;
                        ((TextBlock)sender).Visibility = Visibility.Collapsed;
                        txt.Focus();
                    };
                    timer.Start();

                }
            }

        }
        private void txtbox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBlock tb = (TextBlock)((Grid)((TextBox)sender).Parent).Children[0];
            tb.Visibility = Visibility.Visible;
            ((TextBox)sender).Visibility = Visibility.Collapsed;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaNameList.Focus();
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var listView = (ListView)sender;
            var gridView = (GridView)listView.View;
            //gridView.Columns[0].Width = Math.Max(listView.ActualWidth - 20, 0);


            Debug.WriteLine(listView.ItemsPanel.Resources.Values);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBlock tb = (TextBlock)((Grid)((TextBox)sender).Parent).Children[0];
                tb.Visibility = Visibility.Visible;
                ((TextBox)sender).Visibility = Visibility.Collapsed;
            }
        }
    }
    public class FixWidth : IValueConverter
    {
        //Computedオブジェクトをその内部の値にコンバート  
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) - 16;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
