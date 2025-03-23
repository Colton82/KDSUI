using KDSUI.Services;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows;
using System.Windows.Controls;

namespace KDSUI.Pages
{
    // Code-behind logic for the Login.xaml UI
    public partial class Login : Page
    {
        // Default constructor initializes the page components
        public Login()
        {
            InitializeComponent();
        }

        // Overloaded constructor that pre-fills username and password fields
        public Login(string username, string password)
        {
            InitializeComponent();
            Username.Text = username;
            Password.Password = password;
        }

        // Handles login button click
        // Sends the username and password to the API and processes the response
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
                    // Set the base address and request headers
                    client.BaseAddress = new Uri("https://localhost:7121/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Send the login request to the API
                    HttpResponseMessage response = await client.PostAsync("api/Users/login", content);

                    // If login succeeds, parse the token and proceed
                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result);

                        // Extract the token from the JSON response
                        string token = jsonResponse.GetProperty("token").GetString() ?? string.Empty;
                        int userId = jsonResponse.GetProperty("id").GetInt32();

                        // Store the user session information globally
                        SessionManager._username = username;
                        SessionManager._jwtToken = token;
                        SessionManager._userId = userId;
                        // Navigate to the dashboard
                        App.DashboardPage = new Dashboard();
                        Application.Current.MainWindow.Content = App.DashboardPage;
                    }

                    // Handle 500-level server errors separately
                    else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        MessageBox.Show("Server error. Please try again later.");
                    }

                    // Handle general authentication failure
                    else
                    {
                        MessageBox.Show("Login failed. Please check your credentials.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle connection or unexpected errors
                MessageBox.Show($"Connection Error: {ex.Message}");
            }
        }

        // Handles register button click
        // Navigates the user to the registration page
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new Register();
        }
    }
}
