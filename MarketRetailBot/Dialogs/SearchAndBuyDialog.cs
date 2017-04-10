using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketRetailBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using RestSharp;

//TODO: need refactor

namespace MarketRetailBot.Dialogs
{
    [Serializable]
    public class SearchAndBuyDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var txt = await argument;
            //start of converstaion or explicit call
            if ((count == 1) || (txt.Text == "/start"))
            {
                PromptDialog.Text(context, PromptForSearchPhrase,
                    "Please type title of shoe you would like to search for.", "Didn't get that, try again");
                return;
            }

            var message = await argument;
            if (message.Text == "/reset")
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to start over?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Keyboard);
            else
                context.Wait(MessageReceivedAsync);
        }


        private async Task PromptForSearchPhrase(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var message = context.MakeMessage();

                var searchString = await result;
                var client = new RestClient("https://kixify-util-services.azurewebsites.net/api/");
                var request = new RestRequest("product/getbytitle", Method.POST);
                request.AddHeader("Authorization", "Basic dXRpbHNlcnZpY2U6dXRpbGF1dGg3Nzc =");
                request.AddObject(new {Title = searchString});
                var resStr = client.Execute(request).Content;

                var products = DeserializeAndUnwrap<List<Product>>(resStr, "Data");


                await context.PostAsync($"We've found {products.Count} results for \"{searchString}\"");

                //Filling up thumbnail cards for representation
                //buttons not working for now
                //     var buttons = new  List<CardAction>() { new CardAction() { Title = "Buy"} };
                //trim
                if (products.Count > 25) products.RemoveRange(25, products.Count - 25);

                var cards = new List<HeroCard>();
                products.ForEach(product =>
                {
                    cards.Add(new HeroCard
                    {
                        Title = product.Title,
                        //  Buttons = buttons,
                        Images = new List<CardImage> {new CardImage(product.MainImageUrl)},
                        Subtitle = product.ColorWay
                    });
                });
                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                var lst = new List<Attachment>();
                cards.ForEach(card => { lst.Add(card.ToAttachment()); });
                message.Attachments = lst;
                await context.PostAsync(message);
            }
            catch (Exception exception)
            {
                await context.PostAsync($"Exception happened: {exception}");
            }
        }

        /// <summary>
        ///     Deserialising json string and unwrapping only part with needed node
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="nameOfNode"></param>
        /// <returns></returns>
        public static T DeserializeAndUnwrap<T>(string json, string nameOfNode)
        {
            var jo = JObject.Parse(json);
            return jo.Properties().First(property => property.Name == nameOfNode).Value.ToObject<T>();
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }
    }
}