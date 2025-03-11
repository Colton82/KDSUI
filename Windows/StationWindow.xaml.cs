using KDSUI.Models;
using KDSUI.UserControls;
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
            foreach (var order in OrderManager.GetOrdersForStation(stationName))
            {
                System.Diagnostics.Debug.WriteLine("Adding order to station window: " + order.ToString());
                Orders.Add(order);
            }

            // Set initial items source for ItemsControl
            OrdersPanel.ItemsSource = pagedOrders;

            UpdatePage();

            // Subscribe to real-time updates (new orders and bumps)
            WebSocketClient.OrderReceived += OnOrderUpdated;
            OrderManager.OrderUpdated += OnOrderUpdated;

            // Enable keyboard navigation
            this.KeyDown += Window_KeyDown;
        }

        private void OnOrderUpdated(DynamicOrderModel order)
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
                    RemoveOrder(order); // Remove order if it moves to another station
                }
            });
        }

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
        /// Enables the arrow keys to control page navigation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return; // Prevents multiple triggers

            if (e.Key == Key.Left && currentPage > 0)
            {
                currentPage--; // Move back only one page
                UpdatePage();
            }
            else if (e.Key == Key.Right && (currentPage + 1) * OrdersPerPage < Orders.Count)
            {
                currentPage++; // Move forward only one page
                UpdatePage();
            }

            e.Handled = true; // Marks the event as handled to prevent repeats
        }



        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WebSocketClient.OrderReceived -= OnOrderUpdated;
            OrderManager.OrderUpdated -= OnOrderUpdated; // Unsubscribe to prevent memory leaks
            this.KeyDown -= Window_KeyDown; // Unsubscribe from key events
        }
    }
}
