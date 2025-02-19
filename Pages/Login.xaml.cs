using KDSUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace KDSUI.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private string username;
        private string password;

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

        /// <summary>
        /// Login button click event, calls API to login user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        // Parse the response content to get the userID
                        string result = await response.Content.ReadAsStringAsync();
                        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result);

                        // Get the userID from the JSON response
                        int userId = jsonResponse.GetProperty("userID").GetInt32();

                        // Store the userID in the session or another appropriate place
                        SessionManager._username = username;
                        SessionManager._userId = userId;
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
                MessageBox.Show("Connection ERR");
            }

        }

        /// <summary>
        /// Register button click event, navigates to the register page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new Register();
        }
    }
}
