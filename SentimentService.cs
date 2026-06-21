namespace Prog_POE_Part_2
{
    internal class SentimentService
    {
    
        // DETECT METHOD
        // Analyzes user input and identifies emotional tone
        // Used to make chatbot responses more human-like
        public string Detect(string input)
        {
            // Convert input to lowercase for consistent matching
            input = input.ToLower();


            // WORRIED / ANXIOUS SENTIMENT
            // Detects fear, stress, or concern in user input
            if (input.Contains("worried") ||
                input.Contains("scared") ||
                input.Contains("anxious") ||
                input.Contains("nervous") ||
                input.Contains("stressed"))
                return "worried";


            // CONFUSED SENTIMENT
            // Detects when user does not understand something
            if (input.Contains("confused") ||
                input.Contains("not sure") ||
                input.Contains("dont understand") ||
                input.Contains("unclear"))
                return "confused";


            // FRUSTRATED SENTIMENT
            // Detects anger or irritation in user input
            if (input.Contains("frustrated") ||
                input.Contains("angry") ||
                input.Contains("annoyed") ||
                input.Contains("irritated"))
                return "frustrated";


            // CURIOUS SENTIMENT
            // Detects interest and curiosity in user input
            if (input.Contains("curious") ||
                input.Contains("interested") ||
                input.Contains("wondering"))
                return "curious";


            // DEFAULT SENTIMENT
            // Used when no emotional keywords are detected
            return "neutral";
        }
    }
}