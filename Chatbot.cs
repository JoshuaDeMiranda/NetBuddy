using System;

// CHATBOT 
namespace Prog_POE_Part_2
{
    
    internal class Chatbot
    {
        // DELEGATE
        public delegate string ResponseHandler(string topic, string userName);

        // SERVICE REFERENCES
        private MemoryService memory;          // remembers name, favourite topic, last topic
        private SentimentService sentiment;    // detects mood (worried, curious, etc.)
        private ResponseService responses;     // holds the cybersecurity topic responses
        private DatabaseService database;      // saves/loads tasks (Part 3)
        private QuizService quiz;              // the mini-game (Part 3)
        private NlpService nlp;                // works out the user's intent (Part 3)
        private ActivityLogService activityLog;// records actions taken (Part 3)

        // Holds the actual method the delegate points to
        private ResponseHandler responseDelegate;

        // PENDING REMINDER STATE
        private int pendingReminderTaskId = -1;     // -1 means "nothing pending"
        private string pendingReminderTitle = "";

        // The intent detected on the most recent message.
        public NlpIntent LastIntent { get; private set; } = NlpIntent.None;

        // CONSTRUCTOR
        // Receives all the services (dependency injection) and links the delegate
        // to the ResponseService's BuildResponse method.
        public Chatbot(MemoryService m, SentimentService s, ResponseService r,
                       DatabaseService db, QuizService q, NlpService n, ActivityLogService log)
        {
            memory = m;
            sentiment = s;
            responses = r;
            database = db;
            quiz = q;
            nlp = n;
            activityLog = log;

            // Point the delegate at the response-building method
            responseDelegate = responses.BuildResponse;
        }

        // Welcome message
        public string GetWelcomeMessage()
        {
            return "Hi, I'm NetBuddy!\n\n" +
                   "Ask me about:\n- Passwords\n- Phishing\n- Malware\n- VPNs\n- Online safety\n\n" +
                   "You can also try:\n" +
                   "- 'Add a task to enable 2FA'\n" +
                   "- 'Remind me to update my password tomorrow'\n" +
                   "- 'Start the quiz'\n" +
                   "- 'Show activity log'\n" +
                   "- 'I like phishing' / 'tell me more'\n\n" +
                   "Or use the Play Quiz Game and View Activity Log buttons on the left!";
        }

        // Main logic
        public string GetResponse(string input)
        {
            // Reset the intent each time so old intents don't leak into a new message
            LastIntent = NlpIntent.None;

            // Lowercase copy of the input - makes keyword matching case-insensitive
            string lower = input.ToLower();

            // Guard against empty/blank input
            if (string.IsNullOrWhiteSpace(input))
                return "Please enter something.";

            // Simple exit message
            if (lower == "exit")
                return $"Stay safe online, {memory.UserName}!";

            // ----------------------------------------------------------
            // PENDING REMINDER FLOW
            // Runs only if user previously added a task
            // ----------------------------------------------------------
            if (pendingReminderTaskId != -1)
            {
                // User declined a reminder
                if (lower.Contains("no"))
                {
                    string title = pendingReminderTitle;
                    pendingReminderTaskId = -1;   // clear the pending state
                    pendingReminderTitle = "";
                    return $"No problem - no reminder set for '{title}'.";
                }

                // Re-run NLP to look for a timeframe like "in 3 days"
                var pendingNlp = nlp.Analyse(input);
                int days = pendingNlp.ReminderDays ?? 0;

                // User agreed ("yes") or gave a number of days
                if (lower.Contains("yes") || days > 0)
                {
                    if (days <= 0) days = 1;   // default to tomorrow

                    // Work out the reminder date and save it to the database
                    DateTime when = DateTime.Now.AddDays(days);
                    database.SetReminder(pendingReminderTaskId, when);

                    string title = pendingReminderTitle;
                    activityLog.Add($"Reminder set: '{title}' for {when:yyyy-MM-dd}");

                    // Clear the pending state
                    pendingReminderTaskId = -1;
                    pendingReminderTitle = "";
                    LastIntent = NlpIntent.AddTask;   // tells the GUI to open the Tasks tab
                    return $"Got it! I'll remind you about '{title}' on {when:yyyy-MM-dd}.";
                }
            }

            // NLP Intent detection
            var analysis = nlp.Analyse(input);
            LastIntent = analysis.Intent;

            // Route the message based on the detected intent
            switch (analysis.Intent)
            {
                case NlpIntent.AddTask:
                    return HandleAddTask(analysis);
                case NlpIntent.ShowTasks:
                    return HandleShowTasks();
                case NlpIntent.StartQuiz:
                    activityLog.Add("Quiz started");
                    return "Let's test your cybersecurity knowledge! Opening the Quiz window for you.";
                case NlpIntent.ShowLog:
                    return activityLog.AsText(7);   // show the last 7 actions in chat
                case NlpIntent.CompleteTask:
                    return "Open the Tasks tab, select a task and click 'Mark Complete' to finish it.";
                case NlpIntent.DeleteTask:
                    return "Open the Tasks tab, select a task and click 'Delete' to remove it.";
            }

            // Detect the user's mood so replies can be more empathetic
            string mood = sentiment.Detect(lower);

            // Memory recall
            if (lower.Contains("what do you remember") ||
                lower.Contains("my interest") ||
                lower.Contains("what am i interested in"))
            {
                if (!string.IsNullOrEmpty(memory.FavouriteTopic))
                    return $"{memory.UserName}, you're interested in {memory.FavouriteTopic}.";
                return $"{memory.UserName}, I don't know your interests yet. Try saying 'I like phishing'.";
            }

            // Favourite topic memory
            if (lower.Contains("i like") || lower.Contains("i love") || lower.Contains("my favourite"))
            {
                // Check if their message mentions a known topic
                foreach (var item in responses.Responses.Keys)
                {
                    if (lower.Contains(item))
                    {
                        memory.FavouriteTopic = item;   // remember it
                        memory.LastTopic = item;
                        return $"Got it {memory.UserName}, I'll remember you're interested in {item}.";
                    }
                }
                return $"Got it {memory.UserName}, I'll remember that!";
            }

            // Follow up Questions to tell me more
            if (lower.Contains("tell me more") || lower.Contains("another tip"))
            {
                if (!string.IsNullOrEmpty(memory.LastTopic))
                {
                    // Build another response on the same topic using the delegate
                    string reply = responseDelegate(memory.LastTopic, memory.UserName);
                    return reply ?? responses.GetRandomResponse(memory.LastTopic);
                }
                return "Ask me a topic first.";
            }

            // Keyword checking for cyber security question
            foreach (var item in responses.Responses)
            {
                if (lower.Contains(item.Key))
                {
                    memory.LastTopic = item.Key;   // remember topic for follow-ups

                    // Build a personalised response; fall back to a random one if needed
                    string reply = responseDelegate(item.Key, memory.UserName);
                    if (string.IsNullOrEmpty(reply))
                        reply = responses.GetRandomResponse(item.Key);

                    // Adjust the tone of the reply based on the detected mood
                    if (mood == "worried")
                        return $"{memory.UserName}, I understand your concern.\n\n{reply}";
                    if (mood == "confused")
                        return $"{memory.UserName}, let me explain clearly.\n\n{reply}";
                    if (mood == "frustrated")
                        return $"{memory.UserName}, take your time - I'm here to help.\n\n{reply}";
                    if (mood == "curious")
                        return $"{memory.UserName}, great question!\n\n{reply}";

                    // No special mood - return the plain reply
                    return reply;
                }
            }

            // Should answer not be found
            return $"I'm not sure about that, {memory.UserName}. " +
                   "Try asking about a cybersecurity topic, or say 'add a task', 'start quiz' or 'show activity log'.";
        }

        // Handles add task
        // Saves a new task to the database. If the user gave a reminder timeframe,
        // it's stored too; otherwise we ask them about a reminder (pending flow).
        private string HandleAddTask(NlpResult analysis)
        {
            // Can't add tasks if the database isn't connected
            if (!database.Available)
                return "I can't reach the task database right now. Please check your MySQL connection settings.";

            // Use the extracted title, or a generic one if none was found
            string title = string.IsNullOrWhiteSpace(analysis.TaskTitle)
                ? "Cybersecurity task" : analysis.TaskTitle;
            string description = $"Cybersecurity task: {title}.";

            // If the user mentioned a timeframe, turn it into a reminder date
            DateTime? reminder = analysis.ReminderDays.HasValue
                ? DateTime.Now.AddDays(analysis.ReminderDays.Value) : (DateTime?)null;

            // Save the task and get back its new database id
            int id = database.AddTask(title, description, reminder);
            if (id == -1)
                return "Sorry, I couldn't save that task. Please try again.";

            // Case 1: a reminder was included in the request
            if (reminder.HasValue)
            {
                activityLog.Add($"Task added: '{title}' (Reminder set for {reminder.Value:yyyy-MM-dd})");
                return $"Task added: '{title}'. I'll remind you on {reminder.Value:yyyy-MM-dd}.";
            }

            // Case 2: no reminder yet - record it as pending and ask the user
            activityLog.Add($"Task added: '{title}' (no reminder set)");
            pendingReminderTaskId = id;       // remember which task we're asking about
            pendingReminderTitle = title;
            return $"Task added: '{title}'. Would you like to set a reminder? (e.g. 'yes, remind me in 3 days')";
        }

        // Handles show tasks
        // Builds a text summary of all saved tasks for the chat window.
        private string HandleShowTasks()
        {
            if (!database.Available)
                return "I can't reach the task database right now. Please check your MySQL connection settings.";

            var tasks = database.GetAllTasks();
            if (tasks.Count == 0)
                return "You have no saved tasks yet. Try 'add a task to enable 2FA'.";

            // Build a numbered list of the tasks
            string text = "Here are your saved tasks:\n";
            int i = 1;
            foreach (var t in tasks)
            {
                text += $"  {i}. {t.Title} - {t.StatusDisplay} (Reminder: {t.ReminderDisplay})\n";
                i++;
            }
            return text.TrimEnd() + "\n\nTip: use the Tasks tab to complete or delete them.";
        }
    }
}