using KDSUI.Pages;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Windows;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace KDSUI.Services
{
    public static class SessionManager
    {
        // Stores the logged-in user's details
        public static int _userId { get; set; }
        public static string _username { get; set; }
        public static string _jwtToken { get; set; }

        /// <summary>
        /// Checks if a user is authenticated by verifying the presence of a token.
        /// </summary>
        public static bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(_jwtToken);
        }

        internal static async void Logout()
        {
            //----------Clear Session Data----------//
            try
            {
                using (var client = new HttpClient())
                {
                    if (client.BaseAddress == null)
                    {
                        client.BaseAddress = new Uri("https://localhost:7121/");
                    }
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager._jwtToken);

                    HttpContent content = new StringContent("", Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("api/Users/logout", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Connection ERR: Logout failed. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection ERR: Logout failed. Please try again.");
            }

            _userId = 0;
            _username = null;
            _jwtToken = null;

            //----------Clear Order Data----------//
            OrderManager.Orders.Clear();
            OrderManager._stationOrders.Clear();
        }
    }
}
