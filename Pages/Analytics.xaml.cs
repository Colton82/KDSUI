using KDSUI.ViewModels;
using System;
using System.Collections.Generic;
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

namespace KDSUI.Pages
{
    // Code-behind for the Analytics.xaml page
    public partial class Analytics : Page
    {
        // Initializes the UI components and binds the ViewModel
        public Analytics()
        {
            InitializeComponent();
        }

        // Called when the user selects a different timeframe from the dropdown
        // Triggers the ViewModel to refresh analytics data based on the new selection
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AnalyticsViewModel viewModel)
            {
                viewModel.LoadAnalytics();
            }
        }
    }
}
