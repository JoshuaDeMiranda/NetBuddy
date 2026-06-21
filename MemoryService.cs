namespace Prog_POE_Part_2
{
    internal class MemoryService
    {
   
        // STORES USER NAME
        // Holds the name entered by the user at startup
        // Used for personalising chatbot responses
        public string UserName { get; set; } = "User";

 
        // STORES FAVOURITE TOPIC
        // Remembers the user's preferred cybersecurity topic
        // Used for personalised responses and memory feature
        public string FavouriteTopic { get; set; } = "";


        // STORES LAST TOPIC DISCUSSED
        // Used for follow-up questions like "tell me more"
        // Helps maintain conversation flow
        public string LastTopic { get; set; } = "";
    }
}