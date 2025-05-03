using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Globalization;
using System.Linq;
using MySql.Data.MySqlClient;
using PlannerApp.Models;

namespace PlannerApp
{
    public partial class MainWindow : Window
    {
        public ICommand OpenTaskCommand { get; private set; }
        // Команда для открытия задачи
        public ObservableCollection<TaskItem> Tasks { get; set; } // Коллекция задач для привязки

        public MainWindow()
        {
            InitializeComponent();

            OpenTaskCommand = new RelayCommand(OpenTask);
            Tasks = new ObservableCollection<TaskItem>(); // Создаём пустой список

            DataContext = this; // Устанавливаем контекст данных

            // Загружаем задачи
            LoadTasks();
        }

        private void LoadTasks()
        {
            DateTime selectedDate = DateTime.Today;
            ToDoTitle.Text = "To do " + selectedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

            Tasks.Clear(); // Очищаем список перед новой загрузкой
            foreach (var task in GetFilteredTasks(selectedDate))
            {
                Tasks.Add(task);
            }
        }

        private void OpenTask(object parameter)
        {
            if (parameter is TaskItem task)
            {
                var tasksWindow = new Tasks(task);
                if (tasksWindow.ShowDialog() == true) // Правильное открытие
                {
                    LoadTasks(); // Обновляем список после сохранения
                }
            }
        }

        private ObservableCollection<TaskItem> GetFilteredTasks(DateTime selectedDate, string status = null)
        {
            var tasks = new ObservableCollection<TaskItem>();

            using (var db = new DatabaseHelper())
            {
                try
                {
                    var conn = db.GetConnection();
                    conn.Open();
                    string query = "SELECT id, title, date, status FROM task WHERE DATE(date) = @selectedDate";

                    if (status != null) // Проверяем, передан ли статус
                    {
                        query += " AND status = @status";
                    }
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@selectedDate", selectedDate.Date);
                        if (status != null)
                        {
                            cmd.Parameters.AddWithValue("@status", status);
                        }
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(new TaskItem
                                {
                                    Id = reader.GetInt32("id"),
                                    Title = reader.GetString("title"),
                                    Date = reader.GetDateTime("date"),
                                    Status = reader.GetString("status")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return tasks;
        }
                private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchText = SearchTextBox.Text;

                using (var db = new DatabaseHelper())
                {
                    var conn = db.GetConnection();
                    conn.Open();
                    
                    string query = @"SELECT * FROM task
                                   WHERE title LIKE @searchText 
                                      OR description LIKE @searchText";
                    
                    var results = new List<TaskItem>();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@searchText", $"%{searchText}%");
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                MessageBox.Show("Ничего не найдено");
                                return;
                            }

                            while (reader.Read())
                            {
                                results.Add(new TaskItem
                                {
                                    Id = reader.GetInt32("id"),
                                    Title = reader.GetString("title"),
                                    Status = reader.GetString("status"),
                                    Date = reader.GetDateTime("date"),
                                    Category = reader.GetString("category")
                                });
                            }
                        }
                    }

                    if (results == null || !results.Any())
                    {
                        MessageBox.Show("Нет результатов");
                        return;
                    }

                    var searchWindow = new Search(results);
                    searchWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }

        private void ShowCalendar(object sender, RoutedEventArgs e)
        {
            Calendar calendarWindow = new Calendar();
            calendarWindow.Show();
            this.Close();
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

        private void Logout(object sender, RoutedEventArgs e)
        {
            LoginWindow categoryWindow = new LoginWindow();
            categoryWindow.Show();
            this.Close();
        }

       
    }

  
}
