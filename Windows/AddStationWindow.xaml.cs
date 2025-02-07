using System.Windows;

namespace KDSUI.Windows
{
    public partial class AddStationWindow : Window
    {
        public string StationName { get; private set; }

        public AddStationWindow()
        {
            InitializeComponent();
        }

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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
