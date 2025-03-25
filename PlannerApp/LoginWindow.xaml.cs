using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using PlannerApp; // Подключение пространства имен, если нужно


namespace PlannerApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        // Очистка текста в текстбоксе при фокусе
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text == "Enter Username")
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
                textBox.Text = "Enter Username";
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        // Обработчик для PasswordBox
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Hidden;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password) ? Visibility.Visible : Visibility.Hidden;
        }

        private void PasswordPlaceholder_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PasswordBox.Focus();
        }

        // Кнопка входа
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text;
            string password = PasswordBox.Password;

            if (AuthenticateUser(username, password)) // Проверка аутентификации
            {
                int userId = GetUserId(username); // Получаем userId пользователя

                MessageBox.Show("Login successful!");

                // Создаем окно профиля пользователя и передаем userId
                UserProfileWindow userProfileWindow = new UserProfileWindow(userId);
                userProfileWindow.Show();

                // Закрываем окно входа
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Проверка учетных данных в MySQL
        private bool AuthenticateUser(string username, string password)
        {
            using (var conn = DatabaseHelper.GetConnection()) // Подключение к БД
            {
                try
                {
                    conn.Open();
                    string query = "SELECT password_hash FROM users WHERE username = @Username";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", username);

                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        string storedHash = result.ToString();
                        string enteredHash = HashPassword(password);

                        return storedHash == enteredHash;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            return false;
        }

        // Получение userId пользователя из базы данных
        private int GetUserId(string username)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT id FROM users WHERE username = @Username";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", username);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new Exception("User not found");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error fetching user ID: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return -1; // Возвращаем -1, если произошла ошибка
                }
            }
        }

        // Хеширование пароля (SHA-256)
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        // Переход на регистрацию
        private void SignUp_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}
