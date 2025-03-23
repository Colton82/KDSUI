using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using KDSUI.Models;
using KDSUI.Services;
using Newtonsoft.Json;

namespace KDSUI.Data
{
    public class OrderDAO
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public OrderDAO()
        {
            // Set the base API URL if it hasn't already been configured
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://localhost:7121/");
            }

            // Configure default headers for requests to accept JSON
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Add JWT token for authentication
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager._jwtToken);

            // Ensure event subscription doesn't stack duplicates
            OrderManager.OrdersUpdated -= OnOrdersUpdated;
            OrderManager.OrdersUpdated += OnOrdersUpdated;
        }

        // Fetches all orders associated with the provided username from the API.
        // Adds them to the OrderManager if they aren't already present.
        public async Task GetOrders(string username)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"api/Orders/{username}");
                string jsonResponse = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Raw JSON Response in UI:\n{jsonResponse}");

                var orders = JsonConvert.DeserializeObject<List<DynamicOrderModel>>(jsonResponse);

                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        // Ensure the order's Items collection is initialized
                        order.Items ??= new ObservableCollection<OrderItem>();

                        // Only add the order if it's not already present in the list
                        if (!OrderManager.Orders.Contains(order))
                        {
                            OrderManager.AddOrder(order);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching orders: {ex.Message}");
            }
        }

        // Called whenever OrderManager triggers the OrdersUpdated event.
        // Currently only logs the event to the debug output.
        private void OnOrdersUpdated()
        {
            System.Diagnostics.Debug.WriteLine("OrdersUpdated event triggered.");
        }

        // Sends a completed order to the API for archival storage.
        // Typically called when an order reaches the "Complete" station.
        public async Task ArchiveOrder(DynamicOrderModel order)
        {
            try
            {
                var json = JsonConvert.SerializeObject(order, Formatting.None);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"Sending POST request to API: api/Orders");
                System.Diagnostics.Debug.WriteLine($"Request Payload: {json}");

                HttpResponseMessage response = await _httpClient.PostAsync("api/Orders", content);

                if (!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"Failed to archive order {order.Id}: {response.StatusCode}, API Response: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error archiving order {order.Id}: {ex.Message}");
            }
        }

        // Sends a PUT request to update an existing order in the database.
        // Triggers the OnOrdersUpdated callback if the update is successful.
        public async Task UpdateOrderAsync(DynamicOrderModel order)
        {
            try
            {
                var json = JsonConvert.SerializeObject(order, Formatting.None);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"Sending PUT request to API: api/Orders");
                System.Diagnostics.Debug.WriteLine($"Request Payload: {json}");

                HttpResponseMessage response = await _httpClient.PutAsync("api/Orders", content);

                if (!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"Failed to update order {order.Id}: {response.StatusCode}, API Response: {responseBody}");
                }
                else
                {
                    OnOrdersUpdated();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order {order.Id}: {ex.Message}");
            }
        }

        // Sends an analytics request to the API for the specified timeframe.
        // Returns performance data such as average ticket time and peak hours.
        public async Task<AnalyticsResponse> GetAnalyticsAsync(string timeframe)
        {
            try
            {
                DateTime startDate = timeframe switch
                {
                    "Today" => DateTime.Now.Date,
                    "Past Week" => DateTime.Now.Date.AddDays(-7),
                    "Past Month" => DateTime.Now.Date.AddMonths(-1),
                    _ => DateTime.Now.Date
                };

                string formattedDate = startDate.ToString("yyyy-MM-dd HH:mm:ss");

                var requestBody = new AnalyticsRequest(formattedDate, SessionManager._username);
                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("api/analytics/performance", jsonContent);

                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<AnalyticsResponse>(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching analytics: {ex.Message}");

                return null;
            }
        }
    }

    // Request model for analytics queries
    public class AnalyticsRequest
    {
        public string StartDate { get; set; }
        public string Username { get; set; }

        public AnalyticsRequest(string date, string username)
        {
            StartDate = date;
            Username = username;
        }
    }

    // Response model for analytics data
    public class AnalyticsResponse
    {
        public double AverageTicketTime { get; set; }
        public List<StationPerformance> StationPerformance { get; set; }
        public List<BusiestDay> BusiestDays { get; set; }
        public List<PeakHour> PeakHours { get; set; }
    }

    // Represents performance data for a single station
    public class StationPerformance
    {
        public string Station { get; set; }
        public double Percentage { get; set; }
        public double AvgTime { get; set; }
    }

    // Represents the number of orders placed on a given day
    public class BusiestDay
    {
        public string Day { get; set; }
        public int OrderCount { get; set; }
    }

    // Represents the number of orders received during a specific hour
    public class PeakHour
    {
        public int Hour { get; set; }
        public string FormattedHour => $"{Hour}:00 - {Hour}:59";
        public int OrderCount { get; set; }
    }
}
