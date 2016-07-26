namespace AcademicBot.Conversation
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Connector;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public sealed class HackathonConversationManager : IQueryConversationManager
    {
        // Currently the bot can handle only one query at a time
        private static readonly string QueryName = "academicQuery";
        private static readonly string IsQueryRunning = "IsqueryRunning";

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

        public async Task InitStructuredConjunctiveQueryAsync(List<Predicate> predicates, Activity activity)
        {
            BotData data = await this.GetBotDataAsync(activity);

            HackathonConjunctiveQuery query = new HackathonConjunctiveQuery(predicates);
            string serializedString = JsonConvert.SerializeObject(query.GetAllPredicates());
            data.SetProperty<string>(HackathonConversationManager.QueryName, serializedString);

            await this.SetBotDataAsync(activity, data);
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
            await Task.FromResult(true);
            return "Sorry your query is ambiguous. Curently I am wroking on how to ask clarifying questions to resolve the ambiguity";
        }

        public async Task<bool> ProcessResponseForClarifyingQuestionAsync(Activity activity)
        {
            await Task.FromResult(true);
            return false;
        }

        public async Task<List<Predicate>> GetStructuredConjunctiveQueryAsync(Activity activity)
        {
            BotData data = await this.GetBotDataAsync(activity);
            HackathonConjunctiveQuery query = data.GetProperty<HackathonConjunctiveQuery>(HackathonConversationManager.QueryName);
            return query.GetAllPredicates();
        }

        public async Task EndStructedConjunctiveQueryAsync(Activity activity)
        {
            // do nothing
            await Task.FromResult(true);
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