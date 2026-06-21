using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Prog_POE_Part_2.Models;

namespace Prog_POE_Part_2
{
    public partial class MainWindow : Window
    {
        // SHARED SERVICES (used by chat, Tasks tab, and the popup windows)
        private Chatbot bot;
        private MemoryService memory = new MemoryService();
        private DatabaseService database = new DatabaseService();
        private QuizService quiz = null!;
        private NlpService nlp = new NlpService();
        private ActivityLogService activityLog = new ActivityLogService();

        // References to the separate windows (so we don't open duplicates)
        private GameWindow? gameWindow;
        private LogWindow? logWindow;

        // Reminder pop-up system
        private DispatcherTimer? _reminderTimer;
        private readonly HashSet<int> _alertedReminders = new HashSet<int>();

        public MainWindow()
        {
            InitializeComponent();

            // QuizService 
            quiz = new QuizService(database);

            bot = new Chatbot(
                memory,
                new SentimentService(),
                new ResponseService(),
                database,
                quiz,
                nlp,
                activityLog
            );
        }


        //  Window load
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Fill the reminder time pickers (hours 00-23, minutes in 5-min steps)
                for (int h = 0; h < 24; h++) ReminderHour.Items.Add(h.ToString("D2"));
                for (int m = 0; m < 60; m ++) ReminderMinute.Items.Add(m.ToString("D2"));
                ReminderHour.SelectedItem = "09";
                ReminderMinute.SelectedItem = "00";

                // Plays voice greeting
                string path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Assets", "NetBuddyGreeting.wav");
                
                Voicegreetings voice = new Voicegreetings(path);
                voice.PlayGreeting();

                MessageBox.Show("Hi! Welcome to NetBuddy", "NetBuddy",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                while (true)
                {
                    string name = Microsoft.VisualBasic.Interaction.InputBox(
                        "Enter your name:", "NetBuddy Setup");

                    if (!string.IsNullOrWhiteSpace(name) && IsValidName(name))
                    {
                        memory.UserName = name;
                        break;
                    }
                    MessageBox.Show("Name must contain letters only.");
                }

                await AddBotMessageWithTyping(bot.GetWelcomeMessage());
                await AddBotMessageWithTyping(
                    $"Welcome {memory.UserName}! How can I help you today?");

                //In case of database fail
                if (!database.Available)
                {
                    DbStatusText.Text =
                        "Database not connected: " + database.LastError +
                        "\nThe chatbot and quiz still work - check your database settings in DatabaseService.cs to enable tasks.";
                }

                RefreshTasks();
                StartReminderTimer();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Startup error: " + ex.Message);
            }
        }


        // ============================================================
        //  CHAT
        // ============================================================
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await ProcessUserMessage();
        }

        private async void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await ProcessUserMessage();
        }

        private async Task ProcessUserMessage()
        {
            string input = InputBox.Text;
            if (string.IsNullOrWhiteSpace(input))
                return;

            ChatBox.Items.Add($"{memory.UserName}: {input}");

            string response = bot.GetResponse(input);

            InputBox.Clear();
            await Task.Delay(300);
            await AddBotMessageWithTyping(response);

            RefreshTasks();

            // Open the relevant window based on what the bot understood
            HandleIntentNavigation(bot.LastIntent);
        }

        // Shows task tab
        private void HandleIntentNavigation(NlpIntent intent)
        {
            switch (intent)
            {
                case NlpIntent.StartQuiz:
                    OpenGameWindow();
                    break;
                case NlpIntent.ShowLog:
                    OpenLogWindow();
                    break;
                case NlpIntent.ShowTasks:
                case NlpIntent.AddTask:
                case NlpIntent.CompleteTask:
                case NlpIntent.DeleteTask:
                    MainTabs.SelectedIndex = 1;   // Tasks tab
                    break;
            }
        }

        //Adds typing effect
        private async Task AddBotMessageWithTyping(string message)
        {
            string currentText = "";
            ChatBox.Items.Add("");
            int index = ChatBox.Items.Count - 1;

            foreach (char c in message)
            {
                currentText += c;
                ChatBox.Items[index] = "NetBuddy: " + currentText;
                await Task.Delay(message.Length > 100 ? 5 : 20);
                ChatBox.ScrollIntoView(ChatBox.Items[index]);
            }
        }

        //Checks name valiation
        private bool IsValidName(string name)
        {
            foreach (char c in name)
                if (!char.IsLetter(c) && c != ' ')
                    return false;
            return true;
        }

        //Help button responses
        private async void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            ChatBox.Items.Add($"{memory.UserName}: /help");
            await Task.Delay(300);

            string helpMessage =
                "NetBuddy Help Guide\n\n" +
                "Ask me about: Passwords, Phishing, Malware, VPNs, Online safety.\n\n" +
                "Task commands:\n" +
                "- 'Add a task to enable 2FA'\n" +
                "- 'Remind me to update my password tomorrow'\n" +
                "- 'Show my tasks'\n\n" +
                "Other commands:\n" +
                "- 'Start the quiz' (opens the Quiz window)\n" +
                "- 'Show activity log' (opens the Log window)\n" +
                "- 'I like phishing' / 'tell me more'\n\n" +
                "You can also use the Play Quiz Game and View Activity Log buttons on the left.";

            await AddBotMessageWithTyping(helpMessage);
        }


        // ============================================================
        //  OPEN SEPARATE WINDOWS
        // ============================================================

        // Quiz button
        private void PlayQuizButton_Click(object sender, RoutedEventArgs e)
        {
            OpenGameWindow();
        }

        //Log button
        private void ViewLogButton_Click(object sender, RoutedEventArgs e)
        {
            OpenLogWindow();
        }

        //Handles logic to show game screen
        private void OpenGameWindow()
        {
            if (gameWindow == null)
            {
                gameWindow = new GameWindow(quiz, activityLog) { Owner = this };
                gameWindow.Closed += (s, e) => gameWindow = null;
                gameWindow.Show();
            }
            else
            {
                gameWindow.RestartQuiz();   // already open -> start a fresh game
                gameWindow.Activate();
            }
        }

        //Handles logic to show log screen
        private void OpenLogWindow()
        {
            if (logWindow == null)
            {
                logWindow = new LogWindow(activityLog) { Owner = this };
                logWindow.Closed += (s, e) => logWindow = null;
                logWindow.Show();
            }
            else
            {
                logWindow.Activate();
            }
        }


        // ============================================================
        //  TASK ASSISTANT 
        // ============================================================

        // Adds task button
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (!database.Available)
            {
                // Should database not be connected 
                MessageBox.Show("Database not connected. Check your database settings in DatabaseService.cs.");
                return;
            }

            //Trims user entry
            string title = TaskTitleBox.Text.Trim();
            string desc = TaskDescBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.");
                return;
            }

            DateTime? reminder = null;
            if (ReminderCheck.IsChecked == true && ReminderDate.SelectedDate.HasValue)
            {
                // Combine the chosen date with the chosen hour:minute
                DateTime date = ReminderDate.SelectedDate.Value.Date;
                int hour = ReminderHour.SelectedItem != null
                    ? int.Parse(ReminderHour.SelectedItem.ToString()!) : 9;
                int minute = ReminderMinute.SelectedItem != null
                    ? int.Parse(ReminderMinute.SelectedItem.ToString()!) : 0;
                reminder = date.AddHours(hour).AddMinutes(minute);
            }

            // Adds task to database
            int id = database.AddTask(title, desc, reminder);

            if (id == -1)
            {
                MessageBox.Show("Could not save the task: " + database.LastError);
                return;
            }

            if (reminder.HasValue)
                activityLog.Add($"Task added: '{title}' (Reminder set for {reminder.Value:yyyy-MM-dd HH:mm})");
            else
                activityLog.Add($"Task added: '{title}' (no reminder set)");

            TaskTitleBox.Clear();
            TaskDescBox.Clear();
            ReminderCheck.IsChecked = false;
            ReminderDate.SelectedDate = null;
            ReminderHour.SelectedItem = "09";
            ReminderMinute.SelectedItem = "00";

            RefreshTasks();
        }

        // Marks task as complete
        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TasksGrid.SelectedItem is CyberTask task)
            {
                if (database.MarkComplete(task.Id))
                {
                    activityLog.Add($"Task completed: '{task.Title}'");
                    RefreshTasks();
                }
            }
            else
            {
                MessageBox.Show("Please select a task first.");
            }
        }

        // Deletes task 
        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TasksGrid.SelectedItem is CyberTask task)
            {
                if (database.DeleteTask(task.Id))
                {
                    activityLog.Add($"Task deleted: '{task.Title}'");

                    // Calls refresh database method
                    RefreshTasks();
                }
            }
            else
            {
                MessageBox.Show("Please select a task first.");
            }
        }

        private void RefreshTasksButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshTasks();
        }

        // Shows database
        private void RefreshTasks()
        {
            if (!database.Available)
                return;

            TasksGrid.ItemsSource = null;
            TasksGrid.ItemsSource = database.GetAllTasks();
        }


        // ============================================================
        //  REMINDER POP-UPS
        // ============================================================

        // Starts a timer that checks for due reminders every 30 seconds.
        private void StartReminderTimer()
        {
            _reminderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _reminderTimer.Tick += (s, e) => CheckReminders();
            _reminderTimer.Start();

            // Also check immediately so overdue reminders show on startup
            CheckReminders();
        }

        // Looks for tasks whose reminder time has arrived and pops a message.
        private void CheckReminders()
        {
            if (!database.Available) return;

            DateTime now = DateTime.Now;

            foreach (var task in database.GetAllTasks())
            {
                // Skip completed tasks, tasks without reminders, and ones we've already alerted
                if (task.IsCompleted) continue;
                if (!task.ReminderDate.HasValue) continue;
                if (_alertedReminders.Contains(task.Id)) continue;

                // Has the reminder time arrived (or passed)?
                if (task.ReminderDate.Value <= now)
                {
                    _alertedReminders.Add(task.Id);   // so it only fires once
                    activityLog.Add($"Reminder due: '{task.Title}'");

                    MessageBox.Show(
                        $"Reminder: {task.Title}\n\n{task.Description}",
                        "NetBuddy Reminder",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }
    }
}

/*
Freeman, E. and Robson, E. (2020) Head First Design Patterns. 2nd edn. Sebastopol, CA: O’Reilly Media.
GitHub (2024) GitHub Docs. Available at: https://docs.github.com/ (Accessed: 30 March 2026).
Martin, R.C. (2009) Clean Code: A handbook of agile software craftsmanship. Upper Saddle River, NJ: Prentice Hall.
Microsoft (2024) .NET documentation. Available at: https://learn.microsoft.com/ (Accessed: 30 March 2026).
Troelsen, A. and Japikse, P. (2022) Pro C# 10 with .NET 6: Foundational principles and practices in programming. 11th edn. Berkeley, CA: Apress.
 */