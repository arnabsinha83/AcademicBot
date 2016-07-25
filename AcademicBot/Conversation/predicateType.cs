namespace AcademicBot.Conversation
{
    public enum PredicateType
    {
        Affiliation = 1, // e.g., Microsoft, University of Maryland
        AuthorName = 2,  // e.g., Dharma Shukla
        PaperTitle = 3,  // e.g., 
        PaperTopic = 4,  // e.g., database systems
        PaperVenue = 5,  // e.g., SIGMOD, SIGIR, WWW
        FieldOfStudy = 6, // e.g. machine learning, cancer
        PublicationYear = 7, // e.g., 2007
        Unknown = 8
    }
}