using System;
using System.Text.RegularExpressions;

namespace Prog_POE_Part_2
{
    public enum NlpIntent
    {
        None, AddTask, ShowTasks, CompleteTask, DeleteTask, StartQuiz, ShowLog
    }

    public class NlpResult
    {
        public NlpIntent Intent { get; set; } = NlpIntent.None;
        public string TaskTitle { get; set; } = "";
        public int? ReminderDays { get; set; }
    }

    internal class NlpService
    {
        public NlpResult Analyse(string input)
        {
            var result = new NlpResult();
            string text = input.ToLower().Trim();

            if (text.Contains("activity log") || text.Contains("show log") ||
                text.Contains("what have you done") || text.Contains("recent actions"))
            {
                result.Intent = NlpIntent.ShowLog;
                return result;
            }

            if (text.Contains("quiz") || text.Contains("game") ||
                text.Contains("test me") || text.Contains("test my knowledge"))
            {
                result.Intent = NlpIntent.StartQuiz;
                return result;
            }

            if ((text.Contains("show") || text.Contains("view") ||
                 text.Contains("list") || text.Contains("my")) &&
                 text.Contains("task") && !text.Contains("add"))
            {
                result.Intent = NlpIntent.ShowTasks;
                return result;
            }

            if ((text.Contains("complete") || text.Contains("done") ||
                 text.Contains("finished")) && text.Contains("task"))
            {
                result.Intent = NlpIntent.CompleteTask;
                return result;
            }

            if ((text.Contains("delete") || text.Contains("remove")) && text.Contains("task"))
            {
                result.Intent = NlpIntent.DeleteTask;
                return result;
            }

            if (text.Contains("add task") || text.Contains("add a task") ||
                text.Contains("create task") || text.Contains("create a task") ||
                text.Contains("new task") || text.Contains("remind me") ||
                text.Contains("set a reminder") || text.Contains("set reminder"))
            {
                result.Intent = NlpIntent.AddTask;
                result.TaskTitle = ExtractTaskTitle(input);
                result.ReminderDays = ExtractReminderDays(text);
                return result;
            }

            return result;
        }

        private string ExtractTaskTitle(string input)
        {
            string text = input.Trim();
            string[] leadIns =
            {
                "add a task to", "add task to", "add a task", "add task",
                "create a task to", "create task to", "create a task", "create task",
                "new task to", "new task",
                "remind me to", "remind me",
                "set a reminder to", "set reminder to",
                "set a reminder for", "set reminder for",
                "set a reminder", "set reminder"
            };

            foreach (var phrase in leadIns)
            {
                if (text.ToLower().StartsWith(phrase))
                {
                    text = text.Substring(phrase.Length).Trim();
                    break;
                }
            }

            text = Regex.Replace(text,
                @"\s*(tomorrow|today|in\s+\d+\s+days?|in\s+a\s+week|in\s+\d+\s+weeks?|next\s+week)\s*$",
                "", RegexOptions.IgnoreCase).Trim();

            if (text.Length > 0)
                text = char.ToUpper(text[0]) + text.Substring(1);

            return text;
        }

        private int? ExtractReminderDays(string text)
        {
            if (text.Contains("tomorrow")) return 1;
            if (text.Contains("next week") || text.Contains("in a week")) return 7;

            var dayMatch = Regex.Match(text, @"in\s+(\d+)\s+days?");
            if (dayMatch.Success && int.TryParse(dayMatch.Groups[1].Value, out int days))
                return days;

            var weekMatch = Regex.Match(text, @"in\s+(\d+)\s+weeks?");
            if (weekMatch.Success && int.TryParse(weekMatch.Groups[1].Value, out int weeks))
                return weeks * 7;

            return null;
        }
    }
}