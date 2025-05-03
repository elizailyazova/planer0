using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PlannerApp.Models;

namespace PlannerApp;

public partial class Search : Window 
{
    public Search(IEnumerable<TaskItem> results)
    {
        InitializeComponent();
        
        if (results == null || !results.Any())
        {
            MessageBox.Show("Ошибка: нет данных для отображения");
            this.Close();
            return;
        }

        // Передаем данные в ItemsControl
        SearchResultsList.ItemsSource = results.ToList(); 
    }

    
    private void ShowCalendar(object sender, RoutedEventArgs e)
    {
        Calendar calendarWindow = new Calendar();
        calendarWindow.Show();
        this.Close();
    }

    private void Close()
    {
        throw new System.NotImplementedException();
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