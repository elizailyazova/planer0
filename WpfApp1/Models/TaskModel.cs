namespace WpfApp1.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("tasks")]
public class TaskModel
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public TimeSpan Time { get; set; }
    
    public string? Category { get; set; }
    
    [Required]
    public string Status { get; set; } = "Not Started";
    
    [Column("repeat_type")]
    public string? RepeatType { get; set; }
    
    public int? RecurrenceId { get; set; }
}