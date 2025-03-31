using WpfApp1.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp1.Models
{
    [Table("RecurrenceModel")]
    public class RecurrenceModel
    {
        public int Id { get; set; }
        public int RepeatInterval { get; set; }
        public string Frequency { get; set; }
        public string SelectedDays { get; set; } = "0000000";
        public string SelectedMonths { get; set; } = "000000000000";
        public bool EndsOn { get; set; }
        public DateTime? EndsOnDate { get; set; }
        public bool EndsAfter { get; set; }
        public int? EndsAfterOccurrences { get; set; }
    }
}
