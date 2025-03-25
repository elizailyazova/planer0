using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace PlannerApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Проверяем подключение к базе данных
            if (DatabaseHelper.TestConnection())
            {
                MessageBox.Show("Подключение к БД успешно!", "Статус", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ошибка подключения к БД!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
