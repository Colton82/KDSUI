using KDSUI.Pages;
using KDSUI.Services;
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
using System.Windows.Shapes;

namespace KDSUI.Windows
{
    /// <summary>
    /// Interaction logic for PasswordWindow.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        Page page;
        public PasswordWindow(Page page)
        {
            this.page = page;
            InitializeComponent();
        }

        private async void Done_Click(object sender, RoutedEventArgs e)
        {


            var loginRequest = new
            {
                Username = SessionManager._username,
                Password = PasswordBox.Password
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
                        this.Close();
                        Application.Current.MainWindow.Content = page;

                    }
                    else
                    {
                        MessageBox.Show("Incorrect Password");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection ERR");
            }
        }
    }
}
