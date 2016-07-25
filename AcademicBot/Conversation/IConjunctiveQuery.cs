namespace AcademicBot.Conversation
{
    using System.Collections.Generic;

    interface IConjunctiveQuery
    {
        void AddPredicate(Predicate predicate);

        void RemovePredicate(Predicate predicate);

        /// <summary>
        /// Returns true if any two predicates have differnnt Predicate types but same value. 
        /// Othwerwise, returns false.
        /// 
        /// E.g. (AuthorName = "jayanta mondal") AND (PaperTopic = "jayanta mondal")
        /// </summary>
        /// <returns></returns>
        bool IsAmbiguous();

        /// <summary>
        /// Returns a dictionary of ambiguous predicates. Key is the ambiguous predicate value. Value of 
        /// the dictionary is a list of Predicates where the ambiguous value appeared. 
        /// </summary>
        /// <returns></returns>
        Dictionary<string, HashSet<Predicate>> GetAmbiguousPredicates();

        List<Predicate> GetAllPredicates();
    }
}
