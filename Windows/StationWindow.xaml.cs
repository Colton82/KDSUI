using KDSUI.Models;
using KDSUI.UserControls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KDSUI.Windows
{
    public partial class StationWindow : Window
    {
        public string StationName { get; set; }
        public ObservableCollection<OrderModel> Orders { get; set; } = new ObservableCollection<OrderModel>();

        public StationWindow(string stationName)
        {
            InitializeComponent();
            StationName = stationName;
            DataContext = this;

            // Load existing orders for this station
            foreach (var order in OrderManager.GetOrdersForStation(stationName))
            {
                Orders.Add(order);
            }

            UpdateUI();

            // Subscribe to real-time updates (new orders and bumps)
            WebSocketClient.OrderReceived += OnOrderUpdated;
            OrderManager.OrderUpdated += OnOrderUpdated;
        }

        private void OnOrderUpdated(OrderModel order)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (order.Station == StationName)
                {
                    if (!Orders.Contains(order))
                    {
                        Orders.Add(order);
                        UpdateUI();
                    }
                }
                else
                {
                    RemoveOrder(order); // Remove order if it moves to another station
                }
            });
        }

        private void UpdateUI()
        {
            OrdersPanel.Children.Clear();
            foreach (var order in Orders)
            {
                var card = new OrderCard { DataContext = order };
                OrdersPanel.Children.Add(card);
            }
        }

        public void RemoveOrder(OrderModel order)
        {
            if (Orders.Contains(order))
            {
                Orders.Remove(order);
                UpdateUI();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WebSocketClient.OrderReceived -= OnOrderUpdated;
            OrderManager.OrderUpdated -= OnOrderUpdated; // Unsubscribe to prevent memory leaks
        }
    }
}
