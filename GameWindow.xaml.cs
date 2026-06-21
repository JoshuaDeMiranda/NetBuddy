using System;
using System.Windows;
using System.Windows.Controls;

namespace Prog_POE_Part_2
{
    // GAME WINDOW
    public partial class GameWindow : Window
    {
        // The shared quiz logic (questions, scoring, current position)
        private QuizService quiz;

        // The shared activity log, so quiz actions get recorded
        private ActivityLogService activityLog;

        // Stores which answer option the user has selected (-1 means "nothing chosen yet")
        private int selectedOption = -1;

        // CONSTRUCTOR
        // Receives the SHARED quiz and log services from MainWindow, then starts the quiz.
        public GameWindow(QuizService quizService, ActivityLogService log)
        {
            InitializeComponent();      // builds the window's controls from the XAML
            quiz = quizService;
            activityLog = log;
            StartQuiz();                // begin a fresh game as soon as the window opens
        }

        // RESTART QUIZ
        // Public so MainWindow can restart the game if the window is already open.
        public void RestartQuiz()
        {
            StartQuiz();
        }

        // START QUIZ
        // Shuffles/resets the questions, logs the action, and shows the first question.
        private void StartQuiz()
        {
            quiz.StartQuiz();                  // reset score and pick a fresh question order
            activityLog.Add("Quiz started");   // record it in the activity log
            ShowCurrentQuestion();             // display the first question
        }

        // SHOW CURRENT QUESTION
        // Displays the current question and builds a radio button for each answer option.
        private void ShowCurrentQuestion()
        {
            QuizFeedbackText.Text = "";        // clear any feedback from the previous question
            OptionsPanel.Children.Clear();     // remove the old answer buttons
            selectedOption = -1;               // reset the user's choice

            // Get the question the user is currently on (null means the quiz is finished)
            var q = quiz.CurrentQuestion();

            // QUIZ FINISHED
            // If there's no current question, the game is over - show the final score.
            if (q == null)
            {
                QuizQuestionText.Text = quiz.GetFinalFeedback();               // e.g. "Great job!..."
                QuizScoreText.Text = $"Final Score: {quiz.Score}/{quiz.TotalQuestions}";
                SubmitAnswerButton.IsEnabled = false;   // nothing left to submit
                NextQuestionButton.IsEnabled = false;   // nothing left to move to
                activityLog.Add($"Quiz completed - scored {quiz.Score}/{quiz.TotalQuestions}");
                return;
            }

            // Show the running score, current question number, and the question text
            QuizScoreText.Text = $"Score: {quiz.Score}   |   Question {quiz.CurrentNumber} of {quiz.TotalQuestions}";
            QuizQuestionText.Text = q.Question;

            // Build one radio button per answer option
            for (int i = 0; i < q.Options.Count; i++)
            {
                var rb = new RadioButton
                {
                    Content = q.Options[i],                       // the answer text
                    Tag = i,                                      // store this option's index in Tag
                    GroupName = "QuizOptions",                    // same group = only one can be picked
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(0, 4, 0, 4),
                    FontSize = 14
                };
                rb.Checked += Option_Checked;     // run Option_Checked when this button is selected
                OptionsPanel.Children.Add(rb);    // add it to the window
            }

            SubmitAnswerButton.IsEnabled = true;   // allow submitting now that options are shown
            NextQuestionButton.IsEnabled = false;  // can't go "next" until they've submitted
        }

        // OPTION CHECKED
        // Runs whenever the user selects a radio button - records which option they picked.
        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            // The picked button is the sender; its Tag holds the option index we stored earlier
            if (sender is RadioButton rb && rb.Tag is int idx)
                selectedOption = idx;
        }

        // SUBMIT ANSWER BUTTON
        // Checks the chosen answer, shows feedback, and updates the score.
        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure the user actually picked an option first
            if (selectedOption == -1)
            {
                MessageBox.Show("Please choose an answer first.");
                return;
            }

            var q = quiz.CurrentQuestion();
            if (q == null) return;   // safety check (shouldn't happen mid-game)

            // Ask the quiz service whether the chosen answer is correct
            AnswerResult result = quiz.SubmitAnswer(selectedOption);

            // Show feedback with the explanation either way
            if (result.Correct)
                QuizFeedbackText.Text = "Correct! " + result.Explanation;
            else
                QuizFeedbackText.Text =
                    $"Not quite. The correct answer was: {q.Options[result.CorrectIndex]}.\n{result.Explanation}";

            // Update the running score display
            QuizScoreText.Text = $"Score: {quiz.Score}   |   Question {quiz.CurrentNumber} of {quiz.TotalQuestions}";

            SubmitAnswerButton.IsEnabled = false;   // can't submit the same question twice
            NextQuestionButton.IsEnabled = true;    // now allow moving to the next question
        }

        // NEXT QUESTION BUTTON
        // Advances to the next question and refreshes the display.
        private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            quiz.MoveNext();          // move the quiz forward one question
            ShowCurrentQuestion();    // display it (or the final score if finished)
        }

        // RESTART BUTTON
        // Starts a brand new game from the beginning.
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            StartQuiz();
        }

        // CLOSE BUTTON
        // Closes the quiz window.
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}