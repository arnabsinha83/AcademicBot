﻿using System;
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
using System.Text;
using AcademicBot.Output;

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
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            HackathonConversationManager convManager = HackathonConversationManager.GetInstance();
            StringBuilder replyText = new StringBuilder();
            Activity reply;

            if (activity.Type == ActivityTypes.Message)
            {
                // If needs help
                if (activity.Text.ToUpper().Equals(ConversationConstants.HELP_STRING))
                {
                    replyText.Append(ConversationUtility.GetHelpText());
                }
                // If a valid new query
                else if(activity.Text.ToUpper().Substring(0,ConversationConstants.QUESTION_PREFIX.Length).Equals(ConversationConstants.QUESTION_PREFIX))
                {
                    string query = activity.Text.Substring(ConversationConstants.QUESTION_PREFIX.Length);
                    List<Predicate> predicateList = AcademicApi.CallInterpretMethod(query);
                    await convManager.InitStructuredConjunctiveQueryAsync(predicateList, activity);
                    
                    // If the query is unambiguous
                    if(await convManager.ShouldAskClarifyingQuestionAsync(activity))
                    {
                        replyText.Append(await convManager.GetNextClarifyingQuestionAsync(activity));
                    }
                    else
                    {
                        //List<Predicate> structuredQuery = (List<Predicate>)await convManager.GetStructuredConjunctiveQueryAsync(activity);

                        // 1. get query string
                        // 2. call academic api
                        List<Predicate> structuredQueryPredicates = AcademicApi.CallInterpretMethod(query);
                        string structuredQuery = Utilities.GetPredicateConjunction(structuredQueryPredicates);
                        string unformattedResponseText = AcademicApi.CallEvaluateMethod(structuredQuery, 2);
                        // 3. call markdown formatter

                        string formattedResponseText = new JsonFormatter().FormatEvaluateModel(unformattedResponseText);

                        replyText.Append("Here is the list of answers\n");
                        replyText.Append(formattedResponseText);
                    }

                }
                else if(await convManager.IsAQueryInProgress(activity))
                {
                    bool isProcessed = await convManager.ProcessResponseForClarifyingQuestionAsync(activity);

                    if (!isProcessed)
                    {
                        replyText.Append("Sorry, could not understand your response, and will ask you the question again.\n");
                        replyText.Append(await convManager.GetNextClarifyingQuestionAsync(activity));
                    }
                    else
                    {
                        if (await convManager.ShouldAskClarifyingQuestionAsync(activity))
                        {
                            replyText.Append(await convManager.GetNextClarifyingQuestionAsync(activity));
                        }
                        else
                        {
                            List<Predicate> structuredQuery = (List<Predicate>)await convManager.GetStructuredConjunctiveQueryAsync(activity);
                            // 1. get query string
                            // 2. call academic api
                            // 3. call markdown formatter

                            replyText.Append("Here is the list of answers");
                        }
                    }
                }
                else
                {
                    replyText.Append(ConversationUtility.GetIntroText()); 
                }

                reply = activity.CreateReply(replyText.ToString());
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