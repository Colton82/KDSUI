using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
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
using System.Text.Json;
using KDSUI.Windows;

namespace KDSUI.Pages
{
    /// <summary>
    /// Interaction logic for EditLayout.xaml
    /// </summary>
    public partial class EditLayout : Page
    {
        /// <summary>
        /// EditLayout constructor, Initializes the listBox
        /// </summary>
        public EditLayout()
        {
            InitializeComponent();
            StationsList.ItemsSource = LayoutManager.Stations;
        }

        /// <summary>
        /// Delete the selected station
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the station name from the button's CommandParameter
            if (sender is Button deleteButton && deleteButton.CommandParameter is string stationName)
            {
                // Remove the station from the list
                LayoutManager.Stations.Remove(stationName);
                LayoutManager.SaveStationsAsync();

                // Refresh UI to reflect changes
                StationsList.Items.Refresh();
            }
        }


        /// <summary>
        /// Show the AddStationWindow to add a new station
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddStationWindow addStationWindow = new AddStationWindow();

            bool? result = addStationWindow.ShowDialog();

            if (result == true)
            {
                LayoutManager.Stations.Add(addStationWindow.StationName);
                LayoutManager.SaveStationsAsync();
            }
        }

        /// <summary>
        /// Return to the dashboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new Dashboard();
        }


        //--------------------DRAG AND DROP FUNCTIONALITY--------------------//

        private Point _dragStartPoint;

        /// <summary>
        /// Get the starting point of the potential drag operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StationsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        /// <summary>
        /// determine if the mouse has moved far enough to initiate a drag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StationsListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPosition;

                // Check if the mouse has moved far enough to initiate a drag.
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Get the ListBox and the dragged ListBoxItem.
                    ListBox listBox = sender as ListBox;
                    ListBoxItem listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                    if (listBoxItem == null)
                        return;

                    // Retrieve the data item corresponding to the ListBoxItem.
                    string station = (string)listBox.ItemContainerGenerator.ItemFromContainer(listBoxItem);
                    if (station != null)
                    {
                        // Package the data for drag-and-drop.
                        DataObject dragData = new DataObject("myStationFormat", station);
                        DragDrop.DoDragDrop(listBoxItem, dragData, DragDropEffects.Move);
                    }
                }
            }
        }

        /// <summary>
        /// Determine which items to swich on drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StationsListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myStationFormat"))
            {
                string station = e.Data.GetData("myStationFormat") as string;
                ListBox listBox = sender as ListBox;

                // Get the drop position relative to the ListBox.
                Point dropPosition = e.GetPosition(listBox);
                int targetIndex = -1;

                // Determine where in the list the drop occurred.
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    ListBoxItem item = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(i);
                    if (item != null)
                    {
                        // Translate the item's position to the ListBox coordinate space.
                        Point itemPosition = item.TranslatePoint(new Point(0, 0), listBox);
                        if (dropPosition.Y < itemPosition.Y + item.ActualHeight / 2)
                        {
                            targetIndex = i;
                            break;
                        }
                    }
                }
                // If no valid index found, drop at the end.
                if (targetIndex == -1)
                {
                    targetIndex = listBox.Items.Count;
                }

                // Find the original index of the dragged item.
                int oldIndex = LayoutManager.Stations.IndexOf(station);
                if (oldIndex >= 0)
                {
                    // Remove the item first.
                    LayoutManager.Stations.RemoveAt(oldIndex);

                    // If the dragged item was before the drop target, adjust targetIndex.
                    if (oldIndex < targetIndex)
                    {
                        targetIndex--;
                    }

                    // Clamp targetIndex to be within the valid range.
                    targetIndex = Math.Max(0, Math.Min(targetIndex, LayoutManager.Stations.Count));

                    // Insert the item at the new target index.
                    LayoutManager.Stations.Insert(targetIndex, station);
                    System.Diagnostics.Debug.WriteLine(LayoutManager.Stations.ToString());
                    LayoutManager.SaveStationsAsync();
                }
            }
        }

        /// <summary>
        /// Get the ListBoxItem that contains the specified element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <returns></returns>
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T)
                    return (T)current;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
