using System;
using System.Collections.Generic;
using System.Linq;
using Prog_POE_Part_2.Models;

namespace Prog_POE_Part_2
{
    public class AnswerResult
    {
        public bool Correct { get; set; }
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = "";
    }

    public class QuizService
    {
        // Built-in questions: used to SEED the database and as an offline fallback
        private readonly List<QuizQuestion> _fallbackQuestions;

        private List<QuizQuestion> _questions = new List<QuizQuestion>();
        private readonly Random _rand = new Random();
        private DatabaseService _db;

        private int _currentIndex;
        public int Score { get; private set; }
        public bool Started { get; private set; }
        public bool Finished => Started && _currentIndex >= _questions.Count;
        public int TotalQuestions => _questions.Count;
        public int CurrentNumber => Math.Min(_currentIndex + 1, _questions.Count);

        // Now receives the DatabaseService so it can store/read questions
        public QuizService(DatabaseService db)
        {
            _db = db;
            _fallbackQuestions = BuildQuestions();

            // On first run, push the built-in questions into the database
            _db.SeedQuestionsIfEmpty(_fallbackQuestions);
        }

        public void StartQuiz()
        {
            // Try to load questions from the database; fall back to built-in list
            List<QuizQuestion> source = _db.GetQuizQuestions();
            if (source == null || source.Count == 0)
                source = _fallbackQuestions;

            _questions = source.OrderBy(q => _rand.Next()).ToList();
            _currentIndex = 0;
            Score = 0;
            Started = true;
        }

        public QuizQuestion? CurrentQuestion()
        {
            if (!Started || _currentIndex >= _questions.Count)
                return null;
            return _questions[_currentIndex];
        }

        public AnswerResult SubmitAnswer(int selectedIndex)
        {
            var q = _questions[_currentIndex];
            bool correct = selectedIndex == q.CorrectIndex;
            if (correct) Score++;
            return new AnswerResult
            {
                Correct = correct,
                CorrectIndex = q.CorrectIndex,
                Explanation = q.Explanation
            };
        }

        public void MoveNext()
        {
            _currentIndex++;
        }

        public string GetFinalFeedback()
        {
            double ratio = TotalQuestions == 0 ? 0 : (double)Score / TotalQuestions;
            if (ratio >= 0.8)
                return $"Great job! You scored {Score}/{TotalQuestions}. You're a cybersecurity pro!";
            if (ratio >= 0.5)
                return $"Nice work! You scored {Score}/{TotalQuestions}. A little more practice and you'll be unstoppable.";
            return $"You scored {Score}/{TotalQuestions}. Keep learning to stay safe online!";
        }

        // The canonical question bank (defined once, here)
        private List<QuizQuestion> BuildQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "Reply with your password", "Delete the email", "Report it as phishing", "Forward it to friends" },
                    CorrectIndex = 2,
                    Explanation = "Reporting phishing emails helps prevent scams and protects others too." },
                new QuizQuestion {
                    Question = "A strong password should contain personal information like your birthday.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 1, IsTrueFalse = true,
                    Explanation = "Avoid personal info - it's easy for attackers to guess or find online." },
                new QuizQuestion {
                    Question = "What does two-factor authentication (2FA) add to your account?",
                    Options = new List<string> { "A second verification step", "A faster login", "A new password", "More storage" },
                    CorrectIndex = 0,
                    Explanation = "2FA requires a second step (like a code on your phone), making accounts far harder to break into." },
                new QuizQuestion {
                    Question = "Public Wi-Fi networks are always safe to use for online banking.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 1, IsTrueFalse = true,
                    Explanation = "Public Wi-Fi can be intercepted by attackers. Use a VPN for sensitive activities." },
                new QuizQuestion {
                    Question = "Which of these is the safest way to handle a suspicious link?",
                    Options = new List<string> { "Click to see where it goes", "Hover to check the URL first", "Share it to ask friends", "Open it in incognito mode" },
                    CorrectIndex = 1,
                    Explanation = "Hovering reveals the real destination without clicking - never click unknown links." },
                new QuizQuestion {
                    Question = "What is 'social engineering' in cybersecurity?",
                    Options = new List<string> { "Building social networks", "Manipulating people to reveal information", "Coding websites", "Encrypting data" },
                    CorrectIndex = 1,
                    Explanation = "Social engineering tricks people into giving up data, often by creating false urgency or trust." },
                new QuizQuestion {
                    Question = "You should use the same password across all your accounts for convenience.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 1, IsTrueFalse = true,
                    Explanation = "Reusing passwords means one breach exposes every account. Use unique passwords or a password manager." },
                new QuizQuestion {
                    Question = "What does ransomware typically do?",
                    Options = new List<string> { "Speeds up your PC", "Locks your files and demands payment", "Cleans viruses", "Backs up your data" },
                    CorrectIndex = 1,
                    Explanation = "Ransomware encrypts your files and demands money. Regular backups are the best defence." },
                new QuizQuestion {
                    Question = "Keeping your software and operating system updated improves security.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 0, IsTrueFalse = true,
                    Explanation = "Updates patch known vulnerabilities that attackers exploit, so install them promptly." },
                new QuizQuestion {
                    Question = "Which of these is a sign of a phishing email?",
                    Options = new List<string> { "Correct spelling", "A trusted known sender", "Urgent threats and odd links", "A plain greeting" },
                    CorrectIndex = 2,
                    Explanation = "Phishing often uses urgency and suspicious links to pressure you into acting without thinking." },
                new QuizQuestion {
                    Question = "What is the main purpose of a VPN?",
                    Options = new List<string> { "To block all ads", "To encrypt your internet connection", "To speed up downloads", "To delete cookies" },
                    CorrectIndex = 1,
                    Explanation = "A VPN encrypts your traffic, protecting your data especially on public networks." },
                new QuizQuestion {
                    Question = "A password manager is a safe way to store and generate strong passwords.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 0, IsTrueFalse = true,
                    Explanation = "Password managers create and store unique strong passwords so you don't have to remember them all." }
            };
        }
    }
}