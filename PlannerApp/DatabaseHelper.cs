using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using PlannerApp.Models;

namespace PlannerApp
{
    public class DatabaseHelper : IDisposable
    {
        private const string connectionString = "Server=localhost;Database=planer;User ID=root;Password=1234;";
        private MySqlConnection _connection;

        public MySqlConnection Connection => _connection;  // Публичный доступ к _connection


        
        public DatabaseHelper()
        {
            _connection = new MySqlConnection(connectionString);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        // Метод для получения подключения
        public MySqlConnection GetConnection()
        {
            return _connection;  // Возвращаем объект подключения
        }

        public void OpenConnection()
        {
            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при подключении к базе данных: {ex.Message}");
            }
        }

        public TaskItem GetTaskById(int id)
        {
            TaskItem task = null;

            try
            {
                _connection.Open();
                string query = "SELECT id, title, description, date, time, category, repeat_type, status FROM task WHERE id = @id";

                using (var command = new MySqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            task = new TaskItem
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Description = reader.GetString("description"),
                                Date = reader.GetDateTime("date"),
                                Time = reader.GetTimeSpan("time"),
                                Category = reader["category"].ToString(),
                                RepeatType = reader.GetString("repeat_type"),
                                Status = reader.GetString("status")
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
            }
            finally
            {
                _connection?.Close();
            }

            return task;
        }


        public List<TaskItem> GetTasks()
        {
            List<TaskItem> tasks = new List<TaskItem>();

            try
            {
                _connection.Open();
                string query = "SELECT id, title, description, date, time, category, repeat_type, status FROM task";

                using (var command = new MySqlCommand(query, _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskItem
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Description = reader.GetString("description"),
                                Date = reader.GetDateTime("date"),
                                Time = reader.GetTimeSpan("time"),
                                Category = reader["category"].ToString(),
                                RepeatType = reader.GetString("repeat_type"),
                                Status = reader.GetString("status")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
            }
            finally
            {
                _connection?.Close();
            }

            return tasks;
        }

        

        public void SaveChanges(TaskItem task)
        {
            try
            {
                _connection.Open();

                // SQL-запрос для обновления задачи
                string query = "UPDATE task SET title = @title, description = @description, " +
                               "date = @date, time = @time, category = @category, repeat_type = @repeat_type, " +
                               "status = @status WHERE id = @id";

                using (var command = new MySqlCommand(query, _connection))
                {
                    // Передаем значения из объекта task в запрос
                    command.Parameters.AddWithValue("@id", task.Id);
                    command.Parameters.AddWithValue("@title", task.Title);
                    command.Parameters.AddWithValue("@description", task.Description);
                    command.Parameters.AddWithValue("@date", task.Date);
                    command.Parameters.AddWithValue("@time", task.Time);
                    command.Parameters.AddWithValue("@category", task.Category);
                    command.Parameters.AddWithValue("@repeat_type", task.RepeatType);
                    command.Parameters.AddWithValue("@status", task.Status);

                    // Выполняем запрос
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении изменений: {ex.Message}");
            }
            finally
            {
                _connection?.Close();
            }
        }
        

    }
}
