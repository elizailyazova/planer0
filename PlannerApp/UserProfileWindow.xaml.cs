using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace PlannerApp
{
    public partial class UserProfileWindow : Window
    {
        private int userId; // ID пользователя

        public UserProfileWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadUserData();
            
        }

        // Загрузка данных пользователя
        private void LoadUserData()
        {
            using (var db = new DatabaseHelper())
            {
                try
                {
                    // Открываем соединение
                    var conn = db.GetConnection();
                    conn.Open();
                    string query = "SELECT id, first_name, last_name, username, email, contact_number, position, avatar FROM users WHERE id = @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                           
                            UsernameText.Text = reader["username"].ToString();
                            EmailText.Text = reader["email"].ToString();

                            FirstNameBox.Text = reader["first_name"].ToString();
                            LastNameBox.Text = reader["last_name"].ToString();
                            UsernameBox.Text = reader["username"].ToString();
                            EmailBox.Text = reader["email"].ToString();
                            ContactNumberBox.Text = reader["contact_number"].ToString();
                            PositionBox.Text = reader["position"].ToString();


                            // Загрузка аватара
                            if (reader["avatar"] != DBNull.Value)
                            {
                                byte[] imageBytes = (byte[])reader["avatar"];
                                SetAvatarImage(imageBytes);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обновление данных пользователя
        private void UpdateInfo_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new DatabaseHelper())
            {
                try
                {
                    var conn = db.GetConnection();
                    conn.Open();
                    
                    string query = "UPDATE users SET contact_number = @ContactNumber, position = @Position WHERE id = @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ContactNumber", ContactNumberBox.Text);
                    cmd.Parameters.AddWithValue("@Position", PositionBox.Text);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    MessageBox.Show(rowsAffected > 0 ? "The profile has been updated!" : "No changes.", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Изменение аватара (клик по картинке)
        private void AvatarEllipse_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeAvatar();
        }

        // Изменение аватара (по кнопке)
        private void ChangeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeAvatar();
        }

        // Метод для смены аватара
        private void ChangeAvatar()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите изображение",
                Filter = "Файлы изображений|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                SetAvatarImage(imageBytes);
                UpdateAvatarInDatabase(imageBytes);
            }
        }

     
        private void SetAvatarImage(byte[] imageBytes)
        {
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                AvatarImage.ImageSource = bitmap;
                AvatarBrush.ImageSource = bitmap;
            }
        }

        // Обновление аватара в базе данных
        private void UpdateAvatarInDatabase(byte[] imageBytes)
        {
            using (var db = new DatabaseHelper())
            {
                try
                {
                    // Открываем соединение
                    var conn = db.GetConnection();
                    conn.Open();
                    string query = "UPDATE users SET avatar = @Avatar WHERE id = @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Avatar", imageBytes);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Аватар обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении аватара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenCategory(object sender, RoutedEventArgs e)
        {
            Category categoryWindow = new Category();
            categoryWindow.Show();
            this.Close();  
        }

        private void OpenAddTask(object sender, RoutedEventArgs e)
        {
            AddTask addtaskWindow = new AddTask();
            addtaskWindow.Show();
            this.Close();
        }
        private void OpenMain(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }


        private void Logout(object sender, RoutedEventArgs e)
        {
            LoginWindow categoryWindow = new LoginWindow();
            categoryWindow.Show();
            this.Close();
        }


        private void Save_Click(object sender, RoutedEventArgs e) => UpdateInfo_Click(sender, e);
        private void Exit_Click(object sender, RoutedEventArgs e) => Close();
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция смены пароля пока не реализована.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
