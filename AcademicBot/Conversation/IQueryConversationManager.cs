namespace AcademicBot.Conversation
{
    using Microsoft.Bot.Connector;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    interface IQueryConversationManager
    {

        Task<bool> IsAQueryInProgress(Activity activity);

        /// <summary>
        /// Starts conversation for a new query. 
        /// </summary>
        /// <param name="predicates"></param>
        Task InitStructuredConjunctiveQueryAsync(List<Predicate> predicates, Activity activity);

        /// <summary>
        /// Once a query conversation has started, this methods figures out whether 
        /// the query is well-formed or more questions need to be asked to remove the
        /// ambiguity of the query.
        /// 
        /// If this method returns false, that means that the current state of the query is 
        /// unambiguous. 
        /// </summary>
        /// <returns></returns>
        Task<bool> ShouldAskClarifyingQuestionAsync(Activity activity);

        /// <summary>
        /// This methods returns what question needs to be asked. 
        /// </summary>
        /// <returns></returns>
        Task<string> GetNextClarifyingQuestionAsync(Activity activity);

        /// <summary>
        /// This methods process the response for the question asked.
        /// </summary>
        /// <param name="response"></param>
        /// <retunrs>Returns false if response format is invalid. Otherwise, returns true</retunrs>
        Task<bool> ProcessResponseForClarifyingQuestionAsync(Activity activity);

        /// <summary>
        /// Returns the current list of predicate for the query. Note that the query could be ambiguous.
        /// untill claifying questions are required. 
        /// </summary>
        /// <returns></returns>
        Task<List<Predicate>> GetStructuredConjunctiveQueryAsync(Activity activity);

        /// <summary>
        /// Current conversation for the query has been sucessfully processed. 
        /// </summary>
        Task EndStructedConjunctiveQueryAsync(Activity activity);
    }
}