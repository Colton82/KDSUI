using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
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
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://localhost:7121/");
            }
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager._jwtToken);
            OrderManager.OrderUpdated += OnOrderUpdated;
        }

        /// <summary>
        /// Fetches orders from the API and converts them into DynamicOrderModel.
        /// </summary>
        public async Task GetOrders(string username)
        {
            try
            {
                // Send request to API and get response
                HttpResponseMessage response = await _httpClient.GetAsync($"api/Orders/{username}");
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Debugging: Print raw JSON response
                System.Diagnostics.Debug.WriteLine($"Raw JSON Response in UI:\n{jsonResponse}");

                // Deserialize API response into a List<DynamicOrderModel>
                var orders = JsonConvert.DeserializeObject<List<DynamicOrderModel>>(jsonResponse);

                foreach (var order in orders)
                {
                    if (order.Items is not null)
                    {
                        // Ensure `ItemsJson` is properly parsed into Dictionary<string, object>
                        var deserializedItems = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(order.Items));

                        order.Items = deserializedItems;
                    }

                    OrderManager.AddOrder(order);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching orders: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an order in the database.
        /// </summary>
        public async void OnOrderUpdated(DynamicOrderModel order)
        {
            _ = UpdateOrderAsync(order);
        }

        private async Task UpdateOrderAsync(DynamicOrderModel order)
        {
            try
            {

                var updatedOrder = new
                {
                    Id = order.Id,
                    CustomerName = order.CustomerName,
                    Station = order.Station,
                    TimeStamp = order.Timestamp,
                    Items = order.Items
                };

                var json = JsonConvert.SerializeObject(updatedOrder, Formatting.None);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"Sending PUT request to API: api/Orders");
                System.Diagnostics.Debug.WriteLine($"Request Payload: {json}");

                HttpResponseMessage response = await _httpClient.PutAsync("api/Orders", content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to update order {order.Id}: {response.StatusCode}");
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order {order.Id}: {ex.Message}");
            }
        }

    }
}
