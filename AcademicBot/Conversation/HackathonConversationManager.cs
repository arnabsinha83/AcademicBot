namespace AcademicBot.Conversation
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Connector;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Text;

    public sealed class HackathonConversationManager : IQueryConversationManager
    {
        // Currently the bot can handle only one query at a time
        private static readonly string QueryName = "academicQuery";
        private static readonly string IsQueryRunning = "IsqueryRunning";
        private static readonly string QuestionDataText = "question";
        private static readonly string UserQueryText = "userQueryText";

        private static HackathonConversationManager singletonConversationManager;

        private HackathonConversationManager()
        {

        }

        public static HackathonConversationManager GetInstance()
        {
            if (HackathonConversationManager.singletonConversationManager == null)
            {
                HackathonConversationManager.singletonConversationManager = new HackathonConversationManager();
            }

            return HackathonConversationManager.singletonConversationManager;
        }

        public async Task<bool> IsAQueryInProgress(Activity activity)
        {
            BotData data = await this.GetBotDataAsync(activity);
            bool ans = data.GetProperty<bool>(HackathonConversationManager.IsQueryRunning);
            return ans;
        }

        public async Task InitStructuredConjunctiveQueryAsync(List<Predicate> predicates, string userQuery, Activity activity)
        {
            BotData data = await this.GetBotDataAsync(activity);

            HackathonConjunctiveQuery query = new HackathonConjunctiveQuery(predicates);
            string serializedString = JsonConvert.SerializeObject(query.GetAllPredicates());
            data.SetProperty<string>(HackathonConversationManager.QueryName, serializedString);
            data.SetProperty<string>(HackathonConversationManager.UserQueryText, userQuery);
            data.SetProperty<bool>(HackathonConversationManager.IsQueryRunning, true);

            await this.SetBotDataAsync(activity, data);
        }

        public async Task<string> GetUserQueryAsync(Activity activity)
        {
            BotData data = await this.GetBotDataAsync(activity);
            return data.GetProperty<string>(HackathonConversationManager.UserQueryText);
        }

        public async Task<bool> ShouldAskClarifyingQuestionAsync(Activity activity)
        {
            BotData data = await this.GetBotDataAsync(activity);
            HackathonConjunctiveQuery query = this.GetQueryFromBotData(data);
            return query.IsAmbiguous();
        }

        public HackathonConjunctiveQuery GetQueryFromBotData(BotData data)
        {
            List<Predicate> predicates = JsonConvert.DeserializeObject<List<Predicate>>(data.GetProperty<string>(HackathonConversationManager.QueryName));
            return new HackathonConjunctiveQuery(predicates);
        }

        public async Task<string> GetNextClarifyingQuestionAsync(Activity activity)
        {
            BotData data = await this.GetBotDataAsync(activity);
            HackathonConjunctiveQuery query = this.GetQueryFromBotData(data);

            string questionText = "Sorry, I am told that your query to be ambiguous. I am not yet trained for this type of ambiguity. Please, start a new query.\n";
            List<Predicate> ambiguousPredicate;

            foreach (KeyValuePair<PredicateType, HashSet<Predicate>> entry in query.TypeMap)
            {
                // Ambiguity type I, when there are multiple values for same PredicateType
                if (entry.Value.Count > 1)
                {
                    questionText = this.GetQuestionText(entry, out ambiguousPredicate);
                    data.SetProperty<List<Predicate>>(HackathonConversationManager.QuestionDataText, ambiguousPredicate);
                }
            }

            foreach (KeyValuePair<string, HashSet<Predicate>> entry in query.ValueMap)
            {
                // Ambiguity type II, when the same value maps to multiple PredicateType
                if (entry.Value.Count > 1)
                {
                    //TODO
                }
            }

            await this.SetBotDataAsync(activity, data);
            return questionText;
        }

        private string GetQuestionText(KeyValuePair<PredicateType, HashSet<Predicate>> entry, out List<Predicate> ambiguousPredicate)
        {
            StringBuilder questionText = new StringBuilder();

            switch(entry.Key)
            {
                case PredicateType.AuthorName:
                    questionText.Append("Your query matched multiple authors. Plese pick one to proceed");
                    questionText.Append(this.GetQuestionOptions(entry.Value, out ambiguousPredicate));
                    break;

                default:
                    ambiguousPredicate = new List<Predicate>();
                    questionText.Append("Sorry, your query is ambiguous.Curently I am wroking on how to ask clarifying questions to resolve this specific type of ambiguity.");
                    break;
            }

            return questionText.ToString();
        }

        private string GetQuestionOptions(HashSet<Predicate> value, out List<Predicate> ambiguousPredicate)
        {
            StringBuilder sb = new StringBuilder();
            ambiguousPredicate = new List<Predicate>();
            int count = 1; // Make it ABCD

            foreach (Predicate p in value)
            {
                sb.Append(String.Format("({0}) {1}\n", count++, p.Value));
                ambiguousPredicate.Add(p);
            }

            return sb.ToString();
        }
        
        public async Task<bool> ProcessResponseForClarifyingQuestionAsync(Activity activity)
        {
            await Task.FromResult(true);
            BotData data = await this.GetBotDataAsync(activity);
            HackathonConjunctiveQuery query = this.GetQueryFromBotData(data);
            List<Predicate> questionMetadata = data.GetProperty<List<Predicate>>(HackathonConversationManager.QuestionDataText);

            int option;

            if(!int.TryParse(activity.Text, out option))
            {
                return false;
            }
            else // option is an integer
            {
                if(option > 0 && option <= questionMetadata.Count) // option valid
                {
                    for(int i = 0; i < questionMetadata.Count; i++)
                    {
                        if(i != option -1)
                        {
                            query.RemovePredicate(questionMetadata[i]);
                        }
                    }

                    string serializedString = JsonConvert.SerializeObject(query.GetAllPredicates());
                    data.SetProperty<string>(HackathonConversationManager.QueryName, serializedString);
                    await this.SetBotDataAsync(activity, data);
                    return true;
                }
                else // option invalid
                {
                    return false;
                }
            }
        }

        public async Task<List<Predicate>> GetStructuredConjunctiveQueryAsync(Activity activity)
        {
            BotData data = await this.GetBotDataAsync(activity);
            HackathonConjunctiveQuery query = this.GetQueryFromBotData(data);
            return query.GetAllPredicates();
        }

        public async Task EndStructedConjunctiveQueryAsync(Activity activity)
        {
            // do nothing
            BotData data = await this.GetBotDataAsync(activity);
            data.SetProperty<bool>(HackathonConversationManager.IsQueryRunning, false);
            await this.SetBotDataAsync(activity, data);
        }

        private async Task SetBotDataAsync(Activity activity, BotData data)
        {
            StateClient stateClient = activity.GetStateClient();
            await stateClient.BotState.SetConversationDataAsync(activity.ChannelId, activity.From.Id, data);
        }

        private async Task<BotData> GetBotDataAsync(Activity activity)
        {
            StateClient stateClient = activity.GetStateClient();
            BotData conversationData = await stateClient.BotState.GetConversationDataAsync(activity.ChannelId, activity.From.Id);
            return conversationData;
        }
    }
}