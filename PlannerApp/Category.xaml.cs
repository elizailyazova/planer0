using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using PlannerApp.ViewModels;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;

using System.ComponentModel;
using static PlannerApp.Category;



namespace PlannerApp
{
    public partial class Category : Window
    {
        private int userId;
        private string connectionString = "Server=localhost;Database=planer;Uid=root;Pwd=tasa2004;";
        public ObservableCollection<TaskCategory> TaskGroups { get; set; }


        public Category(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            this.DataContext = new CategoryViewModel();
            LoadUserData();
        }

        private void LoadUserData()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT username, email, avatar FROM users WHERE id = @UserId";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            UsernameText.Text = reader["username"].ToString();
                            EmailText.Text = reader["email"].ToString();


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
        private void SetAvatarImage(byte[] imageBytes)
        {
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();


                AvatarBrush.ImageSource = bitmap;
            }
        }

        public Category()
        {
            InitializeComponent();
            TaskGroups = new ObservableCollection<TaskCategory>();
            DataContext = this;
            LoadTasks();
        }



        private void LoadTasks()
        {
            TaskGroups.Clear(); 
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Получаем все категории с задачами в будущем (дата >= сегодня)
                    string categoryQuery = "SELECT DISTINCT category FROM tasks";
                    using (var categoryCmd = new MySqlCommand(categoryQuery, connection))
                    using (var categoryReader = categoryCmd.ExecuteReader())
                    {
                        while (categoryReader.Read())
                        {
                            string category = categoryReader.GetString("category");
                            var taskCategory = new TaskCategory { Category = category };
                            TaskGroups.Add(taskCategory); // Добавляем категорию в список.
                        }
                    }


                    foreach (var taskCategory in TaskGroups)
                    {
                        string taskQuery = "SELECT id, title, date, time FROM tasks WHERE category = @category";
                        using (var taskCmd = new MySqlCommand(taskQuery, connection))
                        {
                            taskCmd.Parameters.AddWithValue("@category", taskCategory.Category);
                            using (var taskReader = taskCmd.ExecuteReader())
                            {
                                while (taskReader.Read())
                                {
                                    var taskItem = new TaskItem
                                    {
                                        Id = taskReader.GetInt32("id"),
                                        Title = taskReader.GetString("title"),
                                        Date = taskReader.GetDateTime("date"),
                                        Time = taskReader.GetTimeSpan("time"),
                                        Category = taskCategory.Category 
                                    };
                                    taskCategory.Tasks.Add(taskItem); 
                                }
                            }
                        }
                    }

                    foreach (var taskGroup in TaskGroups)
                    {
                        Console.WriteLine($"Category: {taskGroup.Category}");
                        foreach (var task in taskGroup.Tasks)
                        {
                            Console.WriteLine($"  - Task: {task.Title}, Date: {task.Date.ToShortDateString()}, Time: {task.Time}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        public class TaskCategory
        {
            public string Category { get; set; }
            public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();
        }

        public class TaskItem
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan Time { get; set; }
            public string Category { get; internal set; }
        }
    }
}
