using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
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

namespace KDSUI.Pages
{
    /// <summary>
    /// registration page
    /// </summary>
    public partial class Register : Page
    {
        public Register()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Register button click event, calls API to register user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = Username.Text;
            string password = Password.Password;

            var registerRequest = new
            {
                Username = username,
                Password = password
            };

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || password != RepeatPassword.Password)
            {
                MessageBox.Show("Your Username/Password is invalid");
                return;
            }

            var json = JsonSerializer.Serialize(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7121/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsync("api/Users/register", content);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Registration successful!");
                }
                else
                {
                    MessageBox.Show("Registration failed. Please check your details.");
                }
            }
        }

        /// <summary>
        /// returns to login page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            Application.Current.MainWindow.Content = login;
        }
    }
}
