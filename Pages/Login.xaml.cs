using KDSUI.Services;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KDSUI.Pages
{
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        public Login(string username, string password)
        {
            InitializeComponent();
            Username.Text = username;
            Password.Password = password;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = Username.Text;
            string password = Password.Password;

            var loginRequest = new
            {
                Username = username,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:7121/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.PostAsync("api/Users/login", content);
                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result);

                        // Retrieve token from response
                        string token = jsonResponse.GetProperty("token").GetString();

                        // Store user session details
                        SessionManager._username = username;
                        SessionManager._jwtToken = token; // Store the JWT token

                        // Navigate to dashboard
                        Dashboard dashboard = new Dashboard();
                        Application.Current.MainWindow.Content = dashboard;
                    }
                    else
                    {
                        MessageBox.Show("Login failed. Please check your credentials.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error: {ex.Message}");
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new Register();
        }
    }
}
