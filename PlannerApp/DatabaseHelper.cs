using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using PlannerApp.Models;


namespace PlannerApp
{
    public class DatabaseHelper
    {
        private const string connectionString = "Server=localhost;Database=planer;User ID=root;Password=tasa2004;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public static bool TestConnection()
        {
            using (var connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Соединение с базой данных установлено успешно.");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при подключении к базе данных: {ex.Message}");
                    return false;
                }
            }
        }

        public static List<TaskItem> GetTasks()
        {
            List<TaskItem> tasks = new List<TaskItem>();

            using (var connection = GetConnection())
            {
                try
                {
                    connection.Open();


                    string query = "SELECT id, title, date, time, category FROM tasks";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                           
                            while (reader.Read())
                            {
                                tasks.Add(new TaskItem
                                {
                                    Title = reader.GetString("title"),
                                    Date = reader.GetDateTime("date"),
                                    Time = TimeSpan.ParseExact(reader.GetString("time"), "hh\\:mm\\:ss", null),
                                    Category = reader["category"].ToString()
                                });

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
                }
            }

            return tasks;
        }
    }
}
