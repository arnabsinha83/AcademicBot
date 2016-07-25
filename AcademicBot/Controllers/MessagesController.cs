using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using AcademicBot.Controllers;
using AcademicBot.Conversation;
using System.Collections.Generic;

namespace AcademicBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply;

                #region Custom code for AcademicBot that talks to the Academic API
                string originalQuery = activity.Text;
                List<Predicate> predicateList = AcademicApi.CallInterpretMethod(originalQuery);

                #region Commenting out the old code
                // Check if the structured query is non-empty
                //if (String.IsNullOrEmpty(structuredQuery))
                //{
                //    reply = activity.CreateReply($"Could not understand the query '{activity.Text}'. Please try a different query.");
                //}
                //else
                //{
                //    // Call evaluate method
                //    string jsonReply = AcademicApi.CallEvaluateMethod(structuredQuery);
                //    reply = activity.CreateReply($"This is what I have:\n\n {jsonReply}");
                //}
                #endregion
                #endregion

                #region Commenting out the old code
                // Check if the structured query is non-empty
                //if (String.IsNullOrEmpty(structuredQuery))
                //{
                //    reply = activity.CreateReply($"Could not understand the query '{activity.Text}'. Please try a different query.");
                //}
                //else
                //{
                //    // Call evaluate method
                //    string jsonReply = AcademicApi.CallEvaluateMethod(structuredQuery);
                //    int count = await UpdateAndGetCounter(activity);
                //    reply = activity.CreateReply($"The number of messages you have sent {count}.\n This is what I have:\n\n {jsonReply}");
                //}
                #endregion

                // return our reply to the user
                reply = activity.CreateReply($"The count of interpretations of the query you sent is {predicateList.Count}.\n");
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private async Task<int> UpdateAndGetCounter(Activity activity)
        {
            StateClient stateClient = activity.GetStateClient();
            BotData conversationData = await stateClient.BotState.GetConversationDataAsync(activity.ChannelId, activity.From.Id);

            int currentCount = conversationData.GetProperty<int>("MessageCounter");
            if (currentCount < 1)
            {
                currentCount = 1;
            }
            else
            {
                currentCount++;
            }

            conversationData.SetProperty<int>("MessageCounter", currentCount);
            await stateClient.BotState.SetConversationDataAsync(activity.ChannelId, activity.From.Id, conversationData);

            return currentCount;
        }
    }
}