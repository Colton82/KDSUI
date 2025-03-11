using KDSUI.Models;
using Newtonsoft.Json;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace KDSUI.UserControls
{
    /// <summary>
    /// Order cards are used to display order information, and are used in the StationView
    /// </summary>
    public partial class OrderCard : UserControl
    {
        private System.Timers.Timer _timer;
        private DateTime _orderTime;

        public DynamicOrderModel Order
        {
            get => DataContext as DynamicOrderModel;
            set => DataContext = value;
        }

        public OrderCard()
        {
            InitializeComponent();
            this.Loaded += OrderCard_Loaded; // Ensure UI loads correctly
        }

        /// <summary>
        /// Runs when the OrderCard is fully loaded
        /// </summary>
        private void OrderCard_Loaded(object sender, RoutedEventArgs e)
        {
            if (Order == null)
            {
                Console.WriteLine("OrderCard DataContext is null!");
                return;
            }

            Console.WriteLine($"Loaded OrderCard for {Order.CustomerName}, items: {JsonConvert.SerializeObject(Order.Items)}");

            string extractedTimestamp = ExtractLatestTimestamp(Order.Timestamp);

            // Initialize Timer
            if (!string.IsNullOrEmpty(extractedTimestamp) && DateTime.TryParse(extractedTimestamp, out _orderTime))
            {
                _timer = new System.Timers.Timer(1000); // Update every second
                _timer.Elapsed += UpdateTimer;
                _timer.Start();
            }
            else
            {
                TimerTextBlock.Text = "Invalid Time";
            }
        }

        /// <summary>
        /// Bump the order to the next station
        /// </summary>
        private void BumpOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Order == null) return;

            string nextStation;
            var currentStationIndex = LayoutManager.Stations.IndexOf(Order.Station);

            if ((currentStationIndex + 1) < LayoutManager.Stations.Count)
                nextStation = LayoutManager.Stations[currentStationIndex + 1];
            else
                nextStation = "Complete";

            OrderManager.UpdateOrderStation(Order, nextStation);
        }

        //--------Timer Methods--------//

        /// <summary>
        /// Updates the timer display
        /// </summary>
        private void UpdateTimer(object sender, ElapsedEventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - _orderTime;

            string formattedTime = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TimerTextBlock.Text = formattedTime;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating timer: {ex.Message}");
            }
        }

        /// <summary>
        /// Extracts the most recent timestamp from the station-prefixed string.
        /// </summary>
        private string ExtractLatestTimestamp(string timeStampString)
        {
            if (string.IsNullOrWhiteSpace(timeStampString)) return null;

            // Split based on station delimiters (" | ") and get the last segment
            string[] parts = timeStampString.Split(" | ", StringSplitOptions.RemoveEmptyEntries);
            return parts.LastOrDefault()?.Trim(); // Get last timestamp part
        }

    }
}