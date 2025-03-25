using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace PlannerApp
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        // Обработчики для TextBox
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Foreground == System.Windows.Media.Brushes.Gray)
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Name == "FirstNameBox") textBox.Text = "Enter First Name";
                if (textBox.Name == "LastNameBox") textBox.Text = "Enter Last Name";
                if (textBox.Name == "UsernameBox") textBox.Text = "Enter Username";
                if (textBox.Name == "EmailBox") textBox.Text = "Enter Email";
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        // Обработчики для PasswordBox
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                if (passwordBox.Name == "PasswordBox") PasswordPlaceholder.Visibility = Visibility.Hidden;
                if (passwordBox.Name == "ConfirmPasswordBox") ConfirmPasswordPlaceholder.Visibility = Visibility.Hidden;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null && string.IsNullOrEmpty(passwordBox.Password))
            {
                if (passwordBox.Name == "PasswordBox") PasswordPlaceholder.Visibility = Visibility.Visible;
                if (passwordBox.Name == "ConfirmPasswordBox") ConfirmPasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }


        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                if (passwordBox.Name == "PasswordBox")
                    PasswordPlaceholder.Visibility = string.IsNullOrEmpty(passwordBox.Password) ? Visibility.Visible : Visibility.Hidden;

                if (passwordBox.Name == "ConfirmPasswordBox")
                    ConfirmPasswordPlaceholder.Visibility = string.IsNullOrEmpty(passwordBox.Password) ? Visibility.Visible : Visibility.Hidden;
            }
        }
        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                ConfirmPasswordPlaceholder.Visibility = string.IsNullOrEmpty(passwordBox.Password) ? Visibility.Visible : Visibility.Hidden;
            }
        }



        private void PasswordPlaceholder_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == PasswordPlaceholder)
                PasswordBox.Focus();
            else if (sender == ConfirmPasswordPlaceholder)
                ConfirmPasswordBox.Focus();
        }

        // Обработчик для кнопки регистрации
        private bool RegisterUser(string firstName, string lastName, string username, string email, string passwordHash)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO users (first_name, last_name, username, email, password_hash) " +
                                   "VALUES (@FirstName, @LastName, @Username, @Email, @PasswordHash)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    int result = cmd.ExecuteNonQuery();
                    return result > 0; 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database error: {ex.Message}");
                    MessageBox.Show("Database connection error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false; // Возвращаем false при ошибке
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameBox.Text;
            string lastName = LastNameBox.Text;
            string username = UsernameBox.Text;
            string email = EmailBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string passwordHash = HashPassword(password);

            if (IsUserExist(email, username))
            {
                MessageBox.Show("User already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (RegisterUser(firstName, lastName, username, email, passwordHash))
            {
                MessageBox.Show("Registration successful!");

                // Переход к окну входа
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                // Закрытие окна регистрации
                this.Close();
            }
            else
            {
                MessageBox.Show("Registration failed. Try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        // Метод для хеширования пароля
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Получаем байтовое представление пароля
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Преобразуем байты в строку (hex)
                StringBuilder builder = new StringBuilder();
                foreach (byte byteValue in bytes)
                {
                    builder.Append(byteValue.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Метод для проверки существования пользователя в базе
        private bool IsUserExist(string email, string username)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM users WHERE email = @Email OR username = @Username";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Username", username);

                    int result = Convert.ToInt32(cmd.ExecuteScalar());
                    return result > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                    return false;
                }
            }
        }
        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            // Создаём экземпляр окна входа и открываем его
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close(); // Закрываем окно регистрации
        }



        private void FirstNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Открытие окна входа
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close(); // Закрыть окно регистрации, если необходимо
        }

    }
}
