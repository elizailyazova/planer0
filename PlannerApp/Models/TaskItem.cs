using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace PlannerApp.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
    
        public string Title { get; set; } = string.Empty;
    
        public string? Description { get; set; }
    
        public DateTime Date { get; set; }
    
        public TimeSpan Time { get; set; }
    
        public string? Category { get; set; }
    
        public string Status { get; set; } = "Not Started";
    
        public string? RepeatType { get; set; }
    
        public int? RecurrenceId { get; set; }
        
                public void Save()
        {
            using (var db = new DatabaseHelper())
            {
                db.Connection.Open();
                string query;

                if (Id > 0)
                {
                    query = @"UPDATE task SET 
                                title = @title, 
                                description = @description, 
                                date = @date, 
                                time = @time, 
                                category = @category, 
                                status = @status 
                              WHERE id = @id";
                }
                else
                {
                    query = @"INSERT INTO task (title, description, date, time, category, status)
                              VALUES (@title, @description, @date, @time, @category, @status)";
                }

                using (var cmd = new MySqlCommand(query, db.Connection))
                {
                    cmd.Parameters.AddWithValue("@title", Title);
                    cmd.Parameters.AddWithValue("@description", Description);
                    cmd.Parameters.AddWithValue("@date", Date);
                    cmd.Parameters.AddWithValue("@time", Time);
                    cmd.Parameters.AddWithValue("@category", Category);
                    cmd.Parameters.AddWithValue("@status", Status);
                    if (Id > 0)
                        cmd.Parameters.AddWithValue("@id", Id);

                    cmd.ExecuteNonQuery();

                    // Получаем ID, если новая задача
                    if (Id == 0)
                        Id = (int)cmd.LastInsertedId;
                }
            }
        }

        public void Delete()
        {
            using (var db = new DatabaseHelper())
            {
                db.Connection.Open();
                string query = "DELETE FROM task WHERE id = @id";

                using (var cmd = new MySqlCommand(query, db.Connection))
                {
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public TaskItem Clone()
        {
            return new TaskItem
            {
                Id = this.Id,
                Title = this.Title,
                Description = this.Description,
                Date = this.Date,
                Time = this.Time,
                Category = this.Category,
                Status = this.Status
            };
        }

    }
    public class TaskCategory
    {
        public string CategoryName { get; set; }
        public List<TaskItem> Tasks { get; set; }
    }

}
