using KDSUI.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Page
    {
        /// <summary>
        /// Dashboard constructor, Initializes the listBox and connects to the WebSocket
        /// </summary>
        public Dashboard()
        {
            InitializeComponent();
            LoadListBox();
            WebSocketClient.ConnectAsync();
            Orders.ItemsSource = WebSocketClient.Orders;
        }

        /// <summary>
        /// EditLayout button click event, navigates to the EditLayout page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditLayout_Click(object sender, RoutedEventArgs e)
        {
            EditLayout editLayout = new EditLayout();
            Application.Current.MainWindow.Content = editLayout;
        }

        /// <summary>
        /// Asynchronously loads the list box with the stations
        /// </summary>
        /// <returns></returns>
        public async Task LoadListBox()
        {
            await LayoutManager.GetStationsAsync();
            StationsList.ItemsSource = LayoutManager.Stations;
        }

        /// <summary>
        /// SelectionChanged event for the StationsList, opens the StationWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StationWindow stationWindow = new(StationsList.SelectedItem.ToString());
            stationWindow.Show();
        }
    }
}
