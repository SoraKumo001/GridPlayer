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
        private Point _startPoint;
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
            var selectedItems = mediaPathList.SelectedItems.Cast<MediaData>().ToList();
            if (selectedItems.Count > 0 && mediaNameList.SelectedIndex >= 0)
            {
                var targetList = settings.mediaList[mediaNameList.SelectedIndex].mediaData;
                foreach (var item in selectedItems)
                {
                    targetList.Remove(item);
                }
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
            if (item.DataContext is MediaDataList list)
            {
                settings.mediaList[0].mediaData.Clear();
                foreach (var d in list.mediaData)
                    settings.mediaList[0].mediaData.Add(d);

                mediaNameList.SelectedIndex = 0;
            }
        }

        private void mediaNameList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Type check is not strictly needed here as we use SelectedIndex, 
            // but let's ensure we only care about the left list if needed.
            // However, this handler is currently empty in its logic.
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
        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mediaNameList.SelectedIndex >= 0)
            {
                string query = searchBox.Text.ToLower();
                var allData = settings.mediaList[mediaNameList.SelectedIndex].mediaData;
                if (string.IsNullOrWhiteSpace(query))
                {
                    mediaPathList.ItemsSource = allData;
                }
                else
                {
                    mediaPathList.ItemsSource = allData.Where(d => d.path.ToLower().Contains(query)).ToList();
                }
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void ListViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = _startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    ListViewItem item = sender as ListViewItem;
                    if (item == null) return;

                    ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
                    if (listView == null) return;

                    DataObject dragData = new DataObject("MediaReorder", item.DataContext);
                    DragDrop.DoDragDrop(item, dragData, DragDropEffects.Move);
                }
            }
        }

        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("MediaReorder") || e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void mediaNameList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("MediaReorder"))
            {
                var droppedData = e.Data.GetData("MediaReorder") as MediaDataList;
                if (droppedData == null) return;

                int targetIndex = GetCurrentIndex(e.GetPosition(mediaNameList), mediaNameList);
                if (targetIndex >= 0)
                {
                    settings.mediaList.Remove(droppedData);
                    if (targetIndex > settings.mediaList.Count) targetIndex = settings.mediaList.Count;
                    settings.mediaList.Insert(targetIndex, droppedData);
                }
            }
        }

        private void mediaPathList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("MediaReorder"))
            {
                var droppedData = e.Data.GetData("MediaReorder") as MediaData;
                if (droppedData == null) return;

                if (!string.IsNullOrWhiteSpace(searchBox.Text)) return; // Disable reorder during search

                if (mediaNameList.SelectedIndex >= 0)
                {
                    var targetList = settings.mediaList[mediaNameList.SelectedIndex].mediaData;
                    int targetIndex = GetCurrentIndex(e.GetPosition(mediaPathList), mediaPathList);
                    if (targetIndex >= 0)
                    {
                        targetList.Remove(droppedData);
                        if (targetIndex > targetList.Count) targetIndex = targetList.Count;
                        targetList.Insert(targetIndex, droppedData);
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && mediaNameList.SelectedIndex >= 0)
                {
                    var targetList = settings.mediaList[mediaNameList.SelectedIndex].mediaData;
                    var mediaFiles = MediaFileHelper.GetAllMediaFiles(files);
                    foreach (string file in mediaFiles)
                    {
                        // Add new media to the list
                        targetList.Add(new MediaData(file, 0));
                    }
                }
            }
        }

        private int GetCurrentIndex(Point pos, ListView listView)
        {
            IInputElement element = listView.InputHitTest(pos);
            if (element != null)
            {
                var container = GetVisualParent<ListViewItem>(element as DependencyObject);
                if (container != null)
                {
                    return listView.ItemContainerGenerator.IndexFromContainer(container);
                }
            }
            return listView.Items.Count;
        }

        private T GetVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
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
