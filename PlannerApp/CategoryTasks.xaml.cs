using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MySql.Data.MySqlClient;
using PlannerApp.Models;

namespace PlannerApp
{
    public partial class CategoryTasks : Window
    {
        public CategoryTasks(string categoryName, List<TaskItem> tasks)
        {
            InitializeComponent();

            // Устанавливаем название категории
            CategoryTitle.Text = categoryName;

            // Устанавливаем задачи для отображения в ListBox
            TaskList.ItemsSource = tasks;
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

    private void OpenCategories(object sender, RoutedEventArgs e)
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
        
        private void ShowCalendar(object sender, RoutedEventArgs e)
        {
            Calendar calendarWindow = new Calendar();
            calendarWindow.Show();
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
