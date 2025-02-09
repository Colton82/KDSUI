using System.Windows;

namespace KDSUI.Windows
{
    /// <summary>
    /// A small pop up window that allows the user to add a new station
    /// </summary>
    public partial class AddStationWindow : Window
    {
        public string StationName { get; private set; }

        public AddStationWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Add the station name to the StationName property and close the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(StationNameTextBox.Text))
            {
                StationName = StationNameTextBox.Text.Trim();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter a station name.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Close the window without adding a station
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
