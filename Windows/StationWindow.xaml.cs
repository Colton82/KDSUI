using KDSUI.Data;
using KDSUI.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KDSUI.Windows
{
    public partial class StationWindow : Window
    {
        public string StationName { get; set; }
        public ObservableCollection<DynamicOrderModel> Orders { get; set; } = new ObservableCollection<DynamicOrderModel>();

        private const int OrdersPerPage = 12;
        private int currentPage = 0;
        private ObservableCollection<DynamicOrderModel> pagedOrders = new ObservableCollection<DynamicOrderModel>();

        public StationWindow(string stationName)
        {
            InitializeComponent();
            StationName = stationName;
            DataContext = this;

            // Load existing orders for this station
            LoadOrders();

            // Bind paged orders to the UI
            OrdersPanel.ItemsSource = pagedOrders;
            UpdatePage();

            // Subscribe to real-time updates
            WebSocketClient.OrderReceived += OnOrderReceived;
            OrderManager.OrdersUpdated += RefreshOrders; // FIX: Changed event signature

            // Enable keyboard navigation
            this.KeyDown += Window_KeyDown;
        }

        /// <summary>
        /// Loads orders dynamically from `OrderManager` for this station.
        /// </summary>
        private void LoadOrders()
        {
            Orders.Clear();
            foreach (var order in OrderManager.GetOrdersForStation(StationName))
            {
                Orders.Add(order);
            }
            UpdatePage();
        }

        /// <summary>
        /// Updates the paginated orders view.
        /// </summary>
        private void UpdatePage()
        {
            pagedOrders.Clear();
            var ordersToShow = Orders.Skip(currentPage * OrdersPerPage).Take(OrdersPerPage).ToList();
            foreach (var order in ordersToShow)
            {
                pagedOrders.Add(order);
            }

            // Update page indicator
            PageIndicator.Text = $"Page {currentPage + 1} of {Math.Max(1, (Orders.Count + OrdersPerPage - 1) / OrdersPerPage)}";
        }

        /// <summary>
        /// Called when a new order is received via WebSocket.
        /// </summary>
        private void OnOrderReceived(DynamicOrderModel order)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (order.Station == StationName)
                {
                    if (!Orders.Contains(order))
                    {
                        Orders.Add(order);
                        UpdatePage();
                    }
                }
                else
                {
                    RemoveOrder(order);
                }
            });
        }

        /// <summary>
        /// Called when `OrdersUpdated` event is triggered to refresh station orders.
        /// </summary>
        private void RefreshOrders()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadOrders(); // Reload station orders
            });
        }

        /// <summary>
        /// Removes an order when it moves to another station.
        /// </summary>
        public void RemoveOrder(DynamicOrderModel order)
        {
            if (Orders.Contains(order))
            {
                Orders.Remove(order);
                UpdatePage();
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdatePage();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage + 1) * OrdersPerPage < Orders.Count)
            {
                currentPage++;
                UpdatePage();
            }
        }

        /// <summary>
        /// Enables the arrow keys to control page navigation.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return;

            if (e.Key == Key.Left && currentPage > 0)
            {
                currentPage--;
                UpdatePage();
            }
            else if (e.Key == Key.Right && (currentPage + 1) * OrdersPerPage < Orders.Count)
            {
                currentPage++;
                UpdatePage();
            }

            e.Handled = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WebSocketClient.OrderReceived -= OnOrderReceived;
            OrderManager.OrdersUpdated -= RefreshOrders;
            this.KeyDown -= Window_KeyDown;
        }
    }
}
