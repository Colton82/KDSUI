using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KDSUI.ViewModels;

namespace KDSUI.Pages
{
    // Code-behind for the EditLayout page
    public partial class EditLayout : Page
    {
        // Stores the initial position where the user clicks before a drag starts
        private Point _dragStartPoint;

        public EditLayout()
        {
            // Initialize UI components and set up the ViewModel
            InitializeComponent();
            DataContext = new EditLayoutViewModel();
        }

        // Records the position of the mouse when the user begins clicking
        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        // Detects if a drag should start based on mouse movement
        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ListBox listBox)
            {
                Point currentPosition = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPosition;

                // Check if the movement is large enough to qualify as a drag
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Start dragging the selected station name
                    if (listBox.SelectedItem is string draggedStation && listBox.DataContext is EditLayoutViewModel viewModel)
                    {
                        viewModel.StartDrag(draggedStation);
                        DragDrop.DoDragDrop(listBox, draggedStation, DragDropEffects.Move);
                    }
                }
            }
        }

        // Handles the drop action when the dragged item is released over the ListBox
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat) && sender is ListBox listBox)
            {
                // Default to placing the item at the end
                int targetIndex = listBox.Items.Count - 1;

                // Find the index where the drop should occur based on mouse position
                Point dropPosition = e.GetPosition(listBox);
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    if (listBox.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem item)
                    {
                        Point itemPosition = item.TranslatePoint(new Point(0, 0), listBox);
                        if (dropPosition.Y < itemPosition.Y + item.ActualHeight / 2)
                        {
                            targetIndex = i;
                            break;
                        }
                    }
                }

                // Notify the ViewModel to handle reordering
                if (listBox.DataContext is EditLayoutViewModel viewModel)
                {
                    viewModel.Drop(targetIndex);
                }
            }
        }
    }
}
