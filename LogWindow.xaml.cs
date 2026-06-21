using System;
using System.Windows;

namespace Prog_POE_Part_2
{
    // LOG WINDOW
    public partial class LogWindow : Window
    {
        // The shared activity log service that holds all the recorded actions
        private ActivityLogService activityLog;

        // Toggle: false = show only the recent actions, true = show the full history
        private bool showFull = false;

        // CONSTRUCTOR
        // Receives the SHARED activity log from MainWindow.
        public LogWindow(ActivityLogService log)
        {
            InitializeComponent();          // build the window's controls from the XAML
            activityLog = log;

            // Subscribe to the log's "Changed" event. Now, whenever a new action is
            // logged anywhere in the app, OnLogChanged runs and refreshes this list.
            activityLog.Changed += OnLogChanged;

            RefreshLog();                   // show whatever's already in the log on open
        }

        // ON LOG CHANGED
        // Runs automatically (via the Changed event) when a new action is logged.
        private void OnLogChanged()
        {
            // The event might fire from a background thread, so Dispatcher.Invoke makes
            // sure the UI update (RefreshLog) runs safely on the UI thread.
            Dispatcher.Invoke(RefreshLog);
        }

        // REFRESH LOG
        // Clears the list and re-fills it with the current log entries.
        private void RefreshLog()
        {
            LogListBox.Items.Clear();   // remove what's currently shown

            // Choose how much to show based on the toggle:
            // full history, or just the most recent 7 actions
            var entries = showFull
                ? activityLog.GetAll()
                : activityLog.GetRecent(7);

            // If nothing has been logged yet, show a friendly placeholder
            if (entries.Count == 0)
            {
                LogListBox.Items.Add("No actions recorded yet.");
                return;
            }

            // Add each entry as a numbered line (1. ..., 2. ..., etc.)
            int i = 1;
            foreach (var entry in entries)
            {
                LogListBox.Items.Add($"{i}. {entry}");
                i++;
            }
        }

        // REFRESH BUTTON
        // Resets back to the short (recent) view and refreshes.
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            showFull = false;
            ShowMoreButton.Content = "Show More";   // reset the toggle button's label
            RefreshLog();
        }

        // SHOW MORE / SHOW LESS BUTTON
        // Flips between showing all actions and only the recent ones.
        private void ShowMoreButton_Click(object sender, RoutedEventArgs e)
        {
            showFull = !showFull;   // flip the toggle

            // Update the button text to match the new state
            ShowMoreButton.Content = showFull ? "Show Less" : "Show More";

            RefreshLog();           // re-display with the new setting
        }

        // CLOSE BUTTON
        // Closes the activity log window.
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // WINDOW CLOSED
        // closed window isn't kept alive in memory and doesn't try to update a window
        private void Window_Closed(object sender, EventArgs e)
        {
            activityLog.Changed -= OnLogChanged;
        }
    }
}