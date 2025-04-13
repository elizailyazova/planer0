using System;
using System.Collections.Generic;

namespace PlannerApp.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Category { get; set; }
        public string RepeatType { get; set; }
        public string Status { get; set; }
    }

    public class TaskCategory
    {
        public string CategoryName { get; set; }
        public List<TaskItem> Tasks { get; set; }
    }

}
