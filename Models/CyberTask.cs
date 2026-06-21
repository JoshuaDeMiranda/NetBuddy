using System;

// Model to store task in database with ID, Title, Description, ReminderDate, Completion Status and when it was created
namespace Prog_POE_Part_2.Models
{
    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ReminderDisplay =>
            ReminderDate.HasValue ? ReminderDate.Value.ToString("yyyy-MM-dd") : "None";

        public string StatusDisplay =>
            IsCompleted ? "Completed" : "Pending";
    }
}