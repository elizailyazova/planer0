using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class TaskService
    {
        private readonly string connectionString = "Server=localhost;Database=planer;User=root;Password=1234;Port=3306;";

        public void AddTask(TaskModel task, RecurrenceModel recurrence = null)
        {
            if (recurrence == null)
            {
                recurrence = new RecurrenceModel { Frequency = "None" }; 
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query =
                    "INSERT INTO Tasks (title, description, date, time, category, repeat_type) VALUES (@title, @description, @date, @time, @category, @repeat_type); SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@title", task.Title);
                    cmd.Parameters.AddWithValue("@description", task.Description);
                    cmd.Parameters.AddWithValue("@date", task.Date);
                    cmd.Parameters.AddWithValue("@time", task.Time);
                    cmd.Parameters.AddWithValue("@category", task.Category);
                    cmd.Parameters.AddWithValue("@repeat_type", recurrence.Frequency); 

                    task.Id = Convert.ToInt32(cmd.ExecuteScalar()); 
                }

                if (recurrence.Frequency != "None")
                {
                    AddRecurrenceDetails(recurrence, connection);
                    GenerateRecurringTasks(task, recurrence, connection);
                }
            }
        }

        private int AddRecurrenceDetails(RecurrenceModel recurrence, MySqlConnection connection)
        {
            string query = "INSERT INTO TaskRecurrence (RepeatInterval, Frequency, SelectedDays, SelectedMonths, EndsOn, EndsOnDate, EndsAfter, EndsAfterOccurrences) " +
                           "VALUES (@RepeatInterval, @Frequency, @SelectedDays, @SelectedMonths, @EndsOn, @EndsOnDate, @EndsAfter, @EndsAfterOccurrences)";
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@RepeatInterval", recurrence.RepeatInterval);
                cmd.Parameters.AddWithValue("@Frequency", recurrence.Frequency);
                cmd.Parameters.AddWithValue("@SelectedDays", recurrence.SelectedDays);
                cmd.Parameters.AddWithValue("@SelectedMonths", recurrence.SelectedMonths);
                cmd.Parameters.AddWithValue("@EndsOn", recurrence.EndsOn);
                cmd.Parameters.AddWithValue("@EndsOnDate", recurrence.EndsOnDate.HasValue ? (object)recurrence.EndsOnDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@EndsAfter", recurrence.EndsAfter);
                cmd.Parameters.AddWithValue("@EndsAfterOccurrences", recurrence.EndsAfterOccurrences.HasValue ? (object)recurrence.EndsAfterOccurrences.Value : DBNull.Value);
                cmd.ExecuteNonQuery();
            }

            string selectQuery = "SELECT LAST_INSERT_ID()";
            using (var cmd = new MySqlCommand(selectQuery, connection))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void GenerateRecurringTasks(TaskModel task, RecurrenceModel recurrence, MySqlConnection connection)
        {
            int recurrenceId = AddRecurrenceDetails(recurrence, connection);
    
            DateTime currentDate = task.Date;
            int count = 1; 
            int? maxOccurrences = recurrence.EndsAfter 
                ? recurrence.EndsAfterOccurrences - 1 
                : null;
    
            DateTime? endDate = recurrence.EndsOn ? recurrence.EndsOnDate : null;

            while ((maxOccurrences == null || count <= maxOccurrences) && 
                   (endDate == null || currentDate <= endDate))
            {
                currentDate = GetNextOccurrenceDate(currentDate, recurrence);
        
                if (endDate.HasValue && currentDate > endDate.Value)
                    break;
            
                AddRecurringTask(task, currentDate, connection, recurrenceId);
                count++;
            }
        }
        
        private DateTime GetNextOccurrenceDate(DateTime currentDate, RecurrenceModel recurrence)
        {
            switch (recurrence.Frequency)
            {
                case "Daily":
                    return currentDate.AddDays(recurrence.RepeatInterval);
                    
                case "Weekly":
                    return GetNextWeeklyDate(currentDate, recurrence);
                    
                case "Monthly":
                    return currentDate.AddMonths(recurrence.RepeatInterval);
                    
                case "Yearly":
                    return GetNextYearlyDate(currentDate, recurrence);
                    
                default:
                    throw new ArgumentException($"Unknown frequency: {recurrence.Frequency}");
            }
        }

        private DateTime GetNextWeeklyDate(DateTime currentDate, RecurrenceModel recurrence)
        {
            if (string.IsNullOrEmpty(recurrence.SelectedDays))
                return currentDate.AddDays(7 * recurrence.RepeatInterval);

            var daysSelected = recurrence.SelectedDays.Select(c => c == '1').ToArray();
            
            DateTime nextDate = currentDate.AddDays(1);
            
            for (int i = 0; i < 7; i++)
            {
                int dayOfWeek = (int)nextDate.DayOfWeek;
                if (daysSelected[dayOfWeek])
                {
                    return nextDate;
                }
                nextDate = nextDate.AddDays(1);
            }
            
            nextDate = currentDate.AddDays(7 * recurrence.RepeatInterval);
            
            for (int i = 0; i < 7; i++)
            {
                int dayOfWeek = (int)nextDate.DayOfWeek;
                if (daysSelected[dayOfWeek])
                {
                    return nextDate;
                }
                nextDate = nextDate.AddDays(1);
            }
            
            return currentDate.AddDays(7 * recurrence.RepeatInterval);
        }

        private DateTime GetNextYearlyDate(DateTime currentDate, RecurrenceModel recurrence)
        {
            if (string.IsNullOrEmpty(recurrence.SelectedMonths)) 
                return currentDate.AddYears(recurrence.RepeatInterval);

            var monthsSelected = recurrence.SelectedMonths.Select(c => c == '1').ToArray();
            
            DateTime nextDate = currentDate.AddMonths(1);
            
            for (int i = 0; i < 12; i++)
            {
                if (monthsSelected[nextDate.Month - 1])
                {
                    return GetValidDateInMonth(nextDate.Year, nextDate.Month, currentDate.Day);
                }
                nextDate = nextDate.AddMonths(1);
            }
            
            nextDate = currentDate.AddYears(recurrence.RepeatInterval);
            
            for (int i = 0; i < 12; i++)
            {
                if (monthsSelected[nextDate.Month - 1])
                {
                    return GetValidDateInMonth(nextDate.Year, nextDate.Month, currentDate.Day);
                }
                nextDate = nextDate.AddMonths(1);
            }
            
            return currentDate.AddYears(recurrence.RepeatInterval);
        }

        private DateTime GetValidDateInMonth(int year, int month, int day)
        {
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int validDay = day <= daysInMonth ? day : daysInMonth;
            
            return new DateTime(year, month, validDay);
        }
        private void AddRecurringTask(TaskModel task, DateTime date, MySqlConnection connection, int recurrenceId)
                {
                    string query = "INSERT INTO Tasks (title, description, date, time, category, repeat_type, RecurrenceId) VALUES (@title, @desc, @date, @time, @category, @repeat, @recurrenceId)";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@title", task.Title);
                        cmd.Parameters.AddWithValue("@desc", task.Description);
                        cmd.Parameters.AddWithValue("@date", date);
                        cmd.Parameters.AddWithValue("@time", task.Time);
                        cmd.Parameters.AddWithValue("@category", task.Category);
                        cmd.Parameters.AddWithValue("@repeat", task.RepeatType); 
                        cmd.Parameters.AddWithValue("@recurrenceId", recurrenceId); 
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
