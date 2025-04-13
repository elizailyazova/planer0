using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlannerApp.Models
{
    
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
