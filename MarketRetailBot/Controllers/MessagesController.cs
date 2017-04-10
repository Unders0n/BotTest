using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Chronic.Handlers;
using MarketRetailBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace MarketRetailBot
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
            //whole new dialog new time here
            // check if activity is of type message
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new SearchAndBuyDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);


            //user data
            //   StateClient stateClient = activity.GetStateClient();
            //    BotData userData =  stateClient.BotState.GetUserData(activity.ChannelId, activity.From.Id);

            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                //if "/start" then asking
                if (activity.Text == "/start")
                {
                    Activity reply = activity.CreateReply($"Welcome to shoe store. Type which shoe you would like to search.");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
               /* if (activity.Text == "/reset")
                {
                    PromptDialog.Confirm(
                     context,
                     AfterResetAsync,
                     "Are you sure you want to reset the count?",
                     "Didn't get that!",
                     promptStyle: PromptStyle.None);
                }*/


                /*
                    // calculate something for us to return
                    int length = (activity.Text ?? string.Empty).Length;

                    // return our reply to the user
                    //   Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                    //   await connector.Conversations.ReplyToActivityAsync(reply);

                       Activity reply = activity.CreateReply($"You  {activity.From.Name} which was {length} characters");
                       await connector.Conversations.ReplyToActivityAsync(reply);

               */


            }
            else
            {
                HandleSystemMessage(activity);
                
                //welcome to newly users registered to bot
                if (activity.Type == ActivityTypes.ContactRelationUpdate)
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                  //var msg = Utils.GetAppSetting("HelloPhrase") ?? "Welcome";
                    var msg = "Hello, this is retail bot. Type the name of show you would like to buy.";
                    Activity reply = activity.CreateReply(msg);
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
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
    }
}