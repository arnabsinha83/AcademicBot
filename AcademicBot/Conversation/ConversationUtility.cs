namespace AcademicBot.Conversation
{
    public sealed class ConversationUtility
    {
        public static string GetHelpText()
        {
            return "You may start a new academic query by first saying \"Q 'your query'\". An example is “Q papers by Alber Einstein”. Here Q stands for a new query. \n If I can’t fully decipher your query I will ask clarifying questions llik, 'Do you mean papers Authored by Albert Einstein (A), or papers from Albert Einstein Institute(B) ?";
        }

        public static string GetIntroText()
        {
            return "To start a conversation with me you need to speak my language :(. Please type 'Help' to see what language I speak";
        }
    }
}