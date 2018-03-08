using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Imie.Bot.Dialogs
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("crypto intent")]
        public async Task CryptoIntent(IDialogContext context, LuisResult result)
        {
            string entities = this.BotEntityRecognition(result);

            string api = await GetFromMarcketCoinApiAsync(entities);

            await context.PostAsync($"Entities: {entities}. retour api: {api}");
            context.Wait(MessageReceived);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }

        public async Task<string> GetFromMarcketCoinApiAsync(string cryptoMoney)
        {
            var client = new HttpClient();

            // Create the HttpContent for the form to be posted.
            var requestContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("text", "This is a block of text"),
            });

            // Get the response.
            HttpResponseMessage response = await client.PostAsync(
                $"https://api.coinmarketcap.com/v1/ticker/{cryptoMoney}",
                requestContent);

            // Get the response content.
            HttpContent responseContent = response.Content;

            string retour = "";

            // Get the stream of the content.
            using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            {
                // Write the output.
                retour = await reader.ReadToEndAsync();
            }
            return retour;
        }

        // Entities found in result
        public string BotEntityRecognition(LuisResult result)
        {
            StringBuilder entityResults = new StringBuilder();

            if (result.Entities.Count > 0)
            {
                foreach (EntityRecommendation item in result.Entities)
                {
                    // Query: Turn on the [light]
                    // item.Type = "HomeAutomation.Device"
                    // item.Entity = "light"
                    entityResults.Append(item.Entity);
                }
                // remove last comma
                //entityResults.Remove(entityResults.Length - 1, 1);
            }

            return entityResults.ToString();
        }
    }

    public class CryptoMoney
    {
        string id;
        string name;
        string symbol;
        double rank;
        double price_usd;
        double price_btc;
        double volume_usd;
        double market_cap_usd;
        double available_supply;
        double total_supply;
        double max_supply;
        double percent_change_1h;
        double percent_change_24h;
        double percent_change_7d;
        DateTime last_updated;
    }
}