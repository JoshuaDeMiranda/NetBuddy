using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Prog_POE_Part_2.Models;

namespace Prog_POE_Part_2
{
    public class DatabaseService
    {
        // SQL Server LocalDB instance (built into Visual Studio).
        // Uses Windows Authentication, so NO username/password is needed.
        private readonly string _database = "netbuddy";

        // Connects to the LocalDB server (master) - used to create the database
        private string ServerConnString =>
            @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;Encrypt=False;";

        // Connects directly to the netbuddy database - used for all normal queries
        private string ConnString =>
            $@"Server=(localdb)\MSSQLLocalDB;Database={_database};Integrated Security=true;Encrypt=False;";

        public bool Available { get; private set; }
        public string LastError { get; private set; } = "";

        public DatabaseService()
        {
            try
            {
                EnsureDatabase();
                Available = true;
            }
            catch (Exception ex)
            {
                Available = false;
                LastError = ex.Message;
            }
        }

        // Creates the database and BOTH tables if they don't already exist
        private void EnsureDatabase()
        {
            // 1) Create the database (connected to the LocalDB server)
            using (var conn = new SqlConnection(ServerConnString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "IF DB_ID('netbuddy') IS NULL CREATE DATABASE netbuddy;", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // 2) Create the tables (connected to the netbuddy database)
            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();

                string createTasks = @"
                    IF OBJECT_ID('tasks','U') IS NULL
                    CREATE TABLE tasks (
                        id            INT IDENTITY(1,1) PRIMARY KEY,
                        title         NVARCHAR(255) NOT NULL,
                        description   NVARCHAR(MAX),
                        reminder_date DATETIME NULL,
                        is_completed  BIT NOT NULL DEFAULT 0,
                        created_at    DATETIME NOT NULL
                    );";
                using (var cmd = new SqlCommand(createTasks, conn))
                    cmd.ExecuteNonQuery();

                string createQuiz = @"
                    IF OBJECT_ID('quiz_questions','U') IS NULL
                    CREATE TABLE quiz_questions (
                        id            INT IDENTITY(1,1) PRIMARY KEY,
                        question      NVARCHAR(500) NOT NULL,
                        options       NVARCHAR(MAX) NOT NULL,
                        correct_index INT NOT NULL,
                        explanation   NVARCHAR(MAX),
                        is_true_false BIT NOT NULL DEFAULT 0
                    );";
                using (var cmd = new SqlCommand(createQuiz, conn))
                    cmd.ExecuteNonQuery();
            }
        }


        // ================= TASK METHODS =================

        public int AddTask(string title, string description, DateTime? reminder)
        {
            try
            {
                using (var conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO tasks (title, description, reminder_date, is_completed, created_at)
                                   VALUES (@title, @desc, @reminder, 0, @created);
                                   SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@desc", (object?)description ?? "");
                        cmd.Parameters.AddWithValue("@reminder", (object?)reminder ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@created", DateTime.Now);
                        object? result = cmd.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return -1;
            }
        }

        // Access all tasks
        public List<CyberTask> GetAllTasks()
        {
            var tasks = new List<CyberTask>();
            try
            {
                using (var conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    string sql = "SELECT id, title, description, reminder_date, is_completed, created_at " +
                                 "FROM tasks ORDER BY created_at DESC;";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new CyberTask
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Title = reader["title"].ToString() ?? "",
                                Description = reader["description"] == DBNull.Value
                                                ? "" : reader["description"].ToString() ?? "",
                                ReminderDate = reader["reminder_date"] == DBNull.Value
                                                ? (DateTime?)null : Convert.ToDateTime(reader["reminder_date"]),
                                IsCompleted = Convert.ToBoolean(reader["is_completed"]),
                                CreatedAt = Convert.ToDateTime(reader["created_at"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
            return tasks;
        }

        //Set reminder
        public bool SetReminder(int taskId, DateTime reminder)
        {
            try
            {
                using (var conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE tasks SET reminder_date = @reminder WHERE id = @id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@reminder", reminder);
                        cmd.Parameters.AddWithValue("@id", taskId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        // Completing task
        public bool MarkComplete(int taskId)
        {
            try
            {
                using (var conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE tasks SET is_completed = 1 WHERE id = @id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        // Delete task
        public bool DeleteTask(int taskId)
        {
            try
            {
                using (var conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "DELETE FROM tasks WHERE id = @id;", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }


        // ================= QUIZ QUESTION METHODS =================

        // Adds Quiz seed
        public void SeedQuestionsIfEmpty(List<QuizQuestion> questions)
        {
            if (!Available) return;
            try
            {
                using (var conn = new SqlConnection(ConnString))
                {
                    conn.Open();

                    int count;
                    using (var countCmd = new SqlCommand("SELECT COUNT(*) FROM quiz_questions;", conn))
                        count = Convert.ToInt32(countCmd.ExecuteScalar());

                    if (count > 0) return;   // already seeded

                    foreach (var q in questions)
                    {
                        string sql = @"INSERT INTO quiz_questions (question, options, correct_index, explanation, is_true_false)
                                       VALUES (@question, @options, @correct, @explanation, @tf);";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            // Options stored as one string joined by '|'
                            cmd.Parameters.AddWithValue("@question", q.Question);
                            cmd.Parameters.AddWithValue("@options", string.Join("|", q.Options));
                            cmd.Parameters.AddWithValue("@correct", q.CorrectIndex);
                            cmd.Parameters.AddWithValue("@explanation", (object?)q.Explanation ?? "");
                            cmd.Parameters.AddWithValue("@tf", q.IsTrueFalse ? 1 : 0);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
        }

        // Accesses Quiz seed
        public List<QuizQuestion> GetQuizQuestions()
        {
            var list = new List<QuizQuestion>();
            if (!Available) return list;
            try
            {
                using (var conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    string sql = "SELECT question, options, correct_index, explanation, is_true_false FROM quiz_questions;";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string opts = reader["options"].ToString() ?? "";
                            list.Add(new QuizQuestion
                            {
                                Question = reader["question"].ToString() ?? "",
                                Options = new List<string>(opts.Split('|')),
                                CorrectIndex = Convert.ToInt32(reader["correct_index"]),
                                Explanation = reader["explanation"] == DBNull.Value
                                                ? "" : reader["explanation"].ToString() ?? "",
                                IsTrueFalse = Convert.ToBoolean(reader["is_true_false"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
            return list;
        }
    }
}