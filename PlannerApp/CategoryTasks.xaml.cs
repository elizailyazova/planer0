using System;
using System.Collections.Generic;
using System.Windows;
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


        private void Logout(object sender, RoutedEventArgs e)
        {
            LoginWindow categoryWindow = new LoginWindow();
            categoryWindow.Show();
            this.Close();
        }
    }
}
