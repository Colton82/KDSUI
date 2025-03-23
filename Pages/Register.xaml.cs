using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KDSUI.Pages
{
    // Code-behind for the Register page
    public partial class Register : Page
    {
        // Initializes the registration page
        public Register()
        {
            InitializeComponent();
        }

        // Handles the Register button click
        // Validates inputs and sends a registration request to the backend API
        public async void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = Username.Text.Trim();
            string password = Password.Password;
            string repeatPassword = RepeatPassword.Password;

            // Perform validation checks
            string validationMessage = ValidatePassword(username, password, repeatPassword);

            if (Password.Password != RepeatPassword.Password)
                validationMessage = "Passwords do not match";

            if (!string.IsNullOrEmpty(validationMessage))
            {
                MessageBox.Show(validationMessage, "Registration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create request body
            var registerRequest = new
            {
                Username = username,
                Password = password
            };

            var json = JsonSerializer.Serialize(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send request to backend API
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7121/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsync("api/Users/register", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Registration successful!");
                    Application.Current.MainWindow.Content = new Login(username, password);
                }
                else
                {
                    MessageBox.Show("Registration failed. Please check your details.");
                }
            }
        }

        // Validates password according to best practices
        private string ValidatePassword(string username, string password, string repeatPassword)
        {
            if (password.Length < 8)
                return "Password must be at least 8 characters long.";

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return "Password must contain at least one uppercase letter.";

            if (!Regex.IsMatch(password, @"[a-z]"))
                return "Password must contain at least one lowercase letter.";

            if (!Regex.IsMatch(password, @"\d"))
                return "Password must contain at least one digit.";

            if (!Regex.IsMatch(password, @"[\W_]"))
                return "Password must contain at least one special character (@, #, $, %, etc.).";

            if (password.ToLower().Contains(username.ToLower()))
                return "Password should not contain the username.";

            if (password != repeatPassword)
                return "Passwords do not match.";

            return null;
        }

        // Validates the username format and constraints
        private string ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "Username cannot be empty.";

            if (username.Length < 5 || username.Length > 20)
                return "Username must be between 5 and 20 characters long.";

            if (!char.IsLetter(username[0]))
                return "Username must start with a letter.";

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9._]+$"))
                return "Username can only contain letters, numbers, underscores, and dots.";

            return null;
        }

        // Handles the Back button click
        // Navigates back to the login page
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            Application.Current.MainWindow.Content = login;
        }

        // Triggered when the password input changes
        // Updates the password strength meter and message
        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordStrength();
        }

        // Calculates and displays the password strength
        private void UpdatePasswordStrength()
        {
            string password = Password.Password;
            string username = Username.Text.Trim();

            // Validate username first
            string usernameValidationMessage = ValidateUsername(username);
            if (!string.IsNullOrEmpty(usernameValidationMessage))
            {
                PasswordStrengthBar.Value = 0;
                PasswordStrengthMessage.Text = usernameValidationMessage;
                PasswordStrengthMessage.Visibility = Visibility.Visible;
                return;
            }

            int strength = 0;
            List<string> missingRequirements = new List<string>();

            // Check minimum length
            if (password.Length >= 8)
                strength++;
            else
                missingRequirements.Add("Password must be at least 8 characters long.");

            // Check uppercase
            if (Regex.IsMatch(password, @"[A-Z]"))
                strength++;
            else
                missingRequirements.Add("Password must contain at least one uppercase letter.");

            // Check lowercase
            if (Regex.IsMatch(password, @"[a-z]"))
                strength++;
            else
                missingRequirements.Add("Password must contain at least one lowercase letter.");

            // Check digit
            if (Regex.IsMatch(password, @"\d"))
                strength++;
            else
                missingRequirements.Add("Password must contain at least one number.");

            // Check special character
            if (Regex.IsMatch(password, @"[\W_]"))
                strength++;
            else
                missingRequirements.Add("Password must contain at least one special character.");

            // Check username inclusion
            if (!string.IsNullOrEmpty(username) && password.ToLower().Contains(username.ToLower()))
            {
                missingRequirements.Add("Password should not contain the username.");
                strength = Math.Max(0, strength - 1);
            }

            // Determine strength label
            string strengthLevel = strength switch
            {
                0 => "Very Weak",
                1 => "Weak",
                2 => "Okay",
                3 => "Good",
                4 => "Strong",
                _ => "Very Strong"
            };

            // Update progress bar and color based on strength
            PasswordStrengthBar.Value = strength;
            PasswordStrengthBar.Foreground = strength switch
            {
                0 or 1 => new SolidColorBrush(Colors.Red),
                2 => new SolidColorBrush(Colors.Orange),
                3 => new SolidColorBrush(Colors.Yellow),
                4 => new SolidColorBrush(Colors.GreenYellow),
                _ => new SolidColorBrush(Colors.Green)
            };

            // Update the strength message based on validation results
            PasswordStrengthMessage.Text = strengthLevel;

            if (missingRequirements.Count > 0)
            {
                PasswordStrengthMessage.Text += $" - {missingRequirements[0]}";
                PasswordStrengthMessage.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordStrengthMessage.Visibility = Visibility.Collapsed;
            }
        }
    }
}
