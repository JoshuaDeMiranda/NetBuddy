using System;
using System.Collections.Generic;
using System.Linq;

// Creates log of recent events that Netbuddy is doing
namespace Prog_POE_Part_2
{
    public class LogEntry
    {
        // Global variables for time and description of log
        public DateTime Time { get; set; }
        public string Description { get; set; } = "";
        public override string ToString() => $"[{Time:HH:mm}] {Description}";
    }

    public class ActivityLogService
    {
        // Creates a list for the log
        private readonly List<LogEntry> _entries = new List<LogEntry>();

        // Raised whenever a new action is logged 
        public event Action? Changed;

        //Adds new log entry
        public void Add(string description)
        {
            _entries.Add(new LogEntry { Time = DateTime.Now, Description = description });
            Changed?.Invoke();
        }

        //Gets recent log from list
        public List<LogEntry> GetRecent(int count = 5)
        {
            return _entries.AsEnumerable().Reverse().Take(count).ToList();
        }

        // Gets all logs from list
        public List<LogEntry> GetAll()
        {
            return _entries.AsEnumerable().Reverse().ToList();
        }

        // Shows number of logs without exposing list 
        public int Count => _entries.Count;

        // Shows 5 logs at a time or returns "no actions recorded yet."
        public string AsText(int count = 5)
        {
            var recent = GetRecent(count);
            if (recent.Count == 0) return "No actions recorded yet.";

            string text = "Here's a summary of recent actions:\n";
            int i = 1;
            foreach (var entry in recent)
            {
                text += $"  {i}. {entry.Description} ({entry.Time:HH:mm})\n";
                i++;
            }
            return text.TrimEnd();
        }
    }
}