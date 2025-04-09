using KDSUI.Models;
using Newtonsoft.Json;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace KDSUI.UserControls
{
    /// <summary>
    /// Represents a single visual order card used in StationView.
    /// Displays customer info, items, and a live timer for time tracking.
    /// </summary>
    public partial class OrderCard : UserControl
    {
        private System.Timers.Timer _timer;
        private DateTime _orderTime;

        // Strongly-typed binding to the order this card represents
        public DynamicOrderModel Order
        {
            get => DataContext as DynamicOrderModel;
            set => DataContext = value;
        }

        // Constructor sets up the UI and hooks the Loaded event
        public OrderCard()
        {
            InitializeComponent();
            this.Loaded += OrderCard_Loaded;
        }

        // Triggered when the card is fully loaded into the visual tree
        private void OrderCard_Loaded(object sender, RoutedEventArgs e)
        {
            if (Order == null)
            {
                Console.WriteLine("OrderCard DataContext is null!");
                return;
            }

            // Debug output for inspecting the order
            Console.WriteLine($"Loaded OrderCard for {Order.CustomerName}, items: {JsonConvert.SerializeObject(Order.Items, Formatting.Indented)}");

            // Extract the most recent timestamp from the order history
            string extractedTimestamp = ExtractLatestTimestamp(Order.Timestamp);

            // If the timestamp is valid, start a live timer
            if (!string.IsNullOrEmpty(extractedTimestamp) && DateTime.TryParse(extractedTimestamp, out _orderTime))
            {
                _timer = new System.Timers.Timer(1000); // Tick every second
                _timer.Elapsed += UpdateTimer;
                _timer.Start();
            }
            else
            {
                TimerTextBlock.Text = "Invalid Time";
            }
        }

        // Triggered when the "Bump" button is clicked
        // Moves the order to the next station or marks it complete
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

        // -------- Timer Methods -------- //

        // Updates the elapsed time shown on the card every second
        private void UpdateTimer(object sender, ElapsedEventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - _orderTime;
            string formattedTime = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds}";

            try
            {
                if (Application.Current != null)
                {
                    // Ensure UI updates are done on the main thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TimerTextBlock.Text = formattedTime;
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating timer: {ex.Message}");
            }
        }

        // Extracts the most recent timestamp from a multi-station history string
        private string ExtractLatestTimestamp(string timeStampString)
        {
            if (string.IsNullOrWhiteSpace(timeStampString)) return null;

            string[] parts = timeStampString.Split(" | ", StringSplitOptions.RemoveEmptyEntries);
            return parts.LastOrDefault()?.Trim();
        }
    }
}
