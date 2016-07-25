namespace AcademicBot.Conversation
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Connector;
    using System.Threading.Tasks;

    public sealed class HackathonConversationManager : IQueryConversationManager
    {
        // Currently the bot can handle only one query at a time
        public const string QueryName = "academicQuery";

        public async Task InitStructuredConjunctiveQueryAsync(List<Predicate> predicates, Activity activity)
        {
            BotData data = await this.GetBotDataAsync(activity);

            HackathonConjunctiveQuery query = new HackathonConjunctiveQuery(predicates);
            data.SetProperty<HackathonConjunctiveQuery>(HackathonConversationManager.QueryName, query);

            await this.SetBotDataAsync(activity, data);
        }

        public async Task<bool> ShouldAskClarifyingQuestionAsync(Activity activity)
        {
            BotData data = await this.GetBotDataAsync(activity);
            HackathonConjunctiveQuery query = data.GetProperty<HackathonConjunctiveQuery>(HackathonConversationManager.QueryName);
            return query.IsAmbiguous();
        }

        public Task<string> GetNextClarifyingQuestionAsync(Activity activity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ProcessResponseForClarifyingQuestionAsync(string response, Activity activity)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Predicate>> GetStructuredConjunctiveQueryAsync(Activity activity)
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