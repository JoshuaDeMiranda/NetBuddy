using System.Collections.Generic;

// Stores Quiz questions and answers
namespace Prog_POE_Part_2.Models
{
    public class QuizQuestion
    {
        public string Question { get; set; } = "";
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = "";
        public bool IsTrueFalse { get; set; }
    }
}