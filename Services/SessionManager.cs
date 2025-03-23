using KDSUI.Pages;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Text;
using Newtonsoft.Json.Linq;

namespace KDSUI.Services
{
    // Handles session management for authenticated users
    public static class SessionManager
    {
        // Stores the currently logged-in user's ID
        public static int _userId { get; set; }

        // Stores the currently logged-in user's username
        public static string _username { get; set; }

        // Stores the JWT token used for authenticated API requests
        public static string _jwtToken { get; set; }

        // Returns true if a JWT token is present, meaning the user is logged in
        public static bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(_jwtToken);
        }

        // Returns the current user's ID
        internal static int GetCurrentUserId()
        {
            return _userId;
        }

        // Logs the user out and clears all session-related data
        internal static async Task LogoutAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Ensure the API base address is set
                    if (client.BaseAddress == null)
                    {
                        client.BaseAddress = new Uri("https://localhost:7121/");
                    }

                    // Prepare headers for JSON and bearer token authentication
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                    // Send logout request to the API
                    HttpContent content = new StringContent("", Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync("api/Users/logout", content).Result;

                    // Display an error message if logout fails
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Connection ERR: Logout failed. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch any unexpected error and show a generic message
                MessageBox.Show("Connection ERR: Logout failed. Please try again.");
            }

            await WebSocketClient.DisconnectAsync();

            // Clear stored session values
            _userId = 0;
            _username = null;
            _jwtToken = null;

            // Clear all in-memory order data (safe on UI thread)
            OrderManager.Orders.Clear();
            OrderManager._stationOrders.Clear();
        }
    }
}
