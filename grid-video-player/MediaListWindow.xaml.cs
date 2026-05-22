using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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
        private Point _startPoint;
        DispatcherTimer? timer;
        public MediaListWindow()
        {
            InitializeComponent();
            DataContext = new MediaListViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // DataContext is already set in Constructor.
        }

        private void mediaNameList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = (ListViewItem)sender;
            if (item.DataContext is MediaDataList list)
            {
                var vm = (MediaListViewModel)DataContext;
                vm.ApplyPlaylistCommand.Execute(list);
            }
        }

        private void mediaNameList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Empty
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timer?.Stop();
            var vm = (MediaListViewModel)DataContext;
            var index = mediaNameList.SelectedIndex;
            if (index >= 0 && e.ClickCount == 1)
            {
                TextBox txt = (TextBox)((Grid)((TextBlock)sender).Parent).Children[1];
                int i;
                for (i = 0; i < vm.Playlists.Count && vm.Playlists[i] != txt.DataContext; i++) ;
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
                    var vm = (MediaListViewModel)DataContext;
                    vm.Playlists.Remove(droppedData);
                    if (targetIndex > vm.Playlists.Count) targetIndex = vm.Playlists.Count;
                    vm.Playlists.Insert(targetIndex, droppedData);
                }
            }
        }

        private void mediaPathList_Drop(object sender, DragEventArgs e)
        {
            var vm = (MediaListViewModel)DataContext;
            if (vm.SelectedPlaylist == null) return;

            if (e.Data.GetDataPresent("MediaReorder"))
            {
                var droppedData = e.Data.GetData("MediaReorder") as MediaData;
                if (droppedData == null) return;

                if (!string.IsNullOrWhiteSpace(searchBox.Text)) return; // Disable reorder during search

                var targetList = vm.SelectedPlaylist.mediaData;
                int targetIndex = GetCurrentIndex(e.GetPosition(mediaPathList), mediaPathList);
                if (targetIndex >= 0)
                {
                    targetList.Remove(droppedData);
                    if (targetIndex > targetList.Count) targetIndex = targetList.Count;
                    targetList.Insert(targetIndex, droppedData);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null)
                {
                    var targetList = vm.SelectedPlaylist.mediaData;
                    var mediaFiles = MediaFileHelper.GetAllMediaFiles(files);
                    foreach (string file in mediaFiles)
                    {
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
