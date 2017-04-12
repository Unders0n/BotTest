using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketRetailBot.ApiHelpers;
using MarketRetailBot.JsonHelpers;
using MarketRetailBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

//TODO: need refactor

namespace MarketRetailBot.Dialogs
{
    //TODO: move to configs

    /// <summary>
    ///     Main dialog flow
    /// </summary>
    [Serializable]
    public class SearchAndBuyDialog : IDialog<object>
    {
        private const string ProductBuyUri = "http://testshop.com/buy/";

        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var txt = await argument;


            //search availability by SKU
            if (txt.Text.Contains(" /searchBySku"))
            {
                var sku = txt.Text.Substring(" /searchBySku".Length);

                var productAvailabilities = await ApiClients.GetProductAvailabilities(sku);

                if (productAvailabilities == null)
                {
                    await context.PostAsync($"None availability found.");
                    return;
                }
                string resultingAvailabilityString = $"Look what we've found for sku {sku}:";

                foreach (var prodAvail in productAvailabilities)
                    resultingAvailabilityString +=
                        $"\n<br/> size: {prodAvail.size}, total price: {prodAvail.total_price}, buy: {ProductBuyUri}{prodAvail.nid}";
                await context.PostAsync(resultingAvailabilityString);

                return;
            }

            //start of converstaion or explicit call
            if ((count == 1) || (txt.Text == "/start"))
            {
                PromptForTitle(context);
                return;
            }

            if (txt.Text == "/reset")
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to start over?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Keyboard);
            else
                context.Wait(MessageReceivedAsync);
        }


        private void PromptForTitle(IDialogContext context)
        {
            PromptDialog.Text(context, PromptForSearchPhrase,
                "Please type title of shoe you would like to search for.", "Didn't get that, try again");
        }


        private async Task PromptForSearchPhrase(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var message = context.MakeMessage();

                var searchString = await result;
                var resStr = ApiClients.GetProducts(searchString);
                var products = SerializationHelper.DeserializeAndUnwrap<List<Product>>(resStr);
                if (products == null)
                {
                    await context.PostAsync($"Nothing found.");
                    PromptForTitle(context);
                    return;
                }

                await context.PostAsync($"We've found {products.Count} results for \"{searchString}\"");

                //Filling up thumbnail cards for representation
                //buttons not working for now

                //trim products for 8 for now
                if (products.Count > 8) products.RemoveRange(8, products.Count - 8);

                var cards = new List<HeroCard>();
                products.ForEach(product =>
                {
                    var plButton = new CardAction
                    {
                        Value = $" /searchBySku {product.Sku}",
                        Type = "imBack",
                        Title = "Show availability"
                    };
                    var buttons = new List<CardAction> {plButton};

                    cards.Add(new HeroCard
                    {
                        Title = product.Title,
                        Buttons = buttons,
                        Images = new List<CardImage> {new CardImage(product.MainImageUrl)},
                        Subtitle = product.ColorWay
                        //  Tap = new CardAction() { }
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