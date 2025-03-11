using KDSUI.Data;
using KDSUI.Models;
using KDSUI.Services;
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
        OrderDAO orderDAO = new OrderDAO();
        /// <summary>
        /// Dashboard constructor, Initializes the listBox and connects to the WebSocket
        /// </summary>
        public Dashboard()
        {
            InitializeComponent();
            LoadListBox();
            WebSocketClient.ConnectAsync();
            OrderManager.OrderUpdated += OnOrderUpdated;
            LoadOrders();
            Orders.ItemsSource = OrderManager.Orders;

            foreach (DynamicOrderModel order in Orders.ItemsSource) {
                System.Diagnostics.Debug.WriteLine(order.Timestamp);
            }
        }

        /// <summary>
        /// Gets Orders from the database and adds them to OrderManager
        /// </summary>
        private void LoadOrders()
        {
            orderDAO.GetOrders(SessionManager._username);
        }


        /// <summary>
        /// OrderUpdated event, updates the Orders list box
        /// </summary>
        /// <param name="model"></param>
        private void OnOrderUpdated(DynamicOrderModel model)
        {
            Orders.ItemsSource = OrderManager.Orders.ToList();
        }

        /// <summary>
        /// EditLayout button click event, navigates to the EditLayout page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditLayout_Click(object sender, RoutedEventArgs e)
        {
            EditLayout editLayout = new EditLayout();
            PasswordWindow passwordBox = new PasswordWindow(editLayout);
            passwordBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            passwordBox.Show();
        }

        /// <summary>
        /// Asynchronously loads the list box with the stations
        /// </summary>
        /// <returns></returns>
        public async void LoadListBox()
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
            try
            {
                StationWindow stationWindow = new(StationsList.SelectedItem.ToString());
                stationWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window != Application.Current.MainWindow)
                {
                    window.Close();
                }
            }
            SessionManager.Logout();
            Application.Current.MainWindow.Content = new Login();
        }
    }
}
