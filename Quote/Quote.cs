using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Quote
{
    public static class Quote
    {
        [FunctionName("Quote")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            IList<string> quotes = new List<string>() {
                "Nothing is impossible. The word itself says ‘I’m possible! - Audrey Hepburn",
                "The bad news is time flies. The good news is you’re the pilot. - Michael Altshuler",
                "Keep your face always toward the sunshine, and shadows will fall behind you. - Walt Whitman"
            };

            SlackResponse response = new SlackResponse()
            {
                ResponseType = "in_channel",
                Text = quotes.RandomSelect()
            };

            return new OkObjectResult(response);
        }

        private static string RandomSelect(this IList<string> list)
        {
            Random random = new Random();
            return list[random.Next(0, list.Count)];
        }
    }

    public class SlackResponse
    {
        //{
        //    "response_type": "in_channel",
        //    "text": "It's 80 degrees right now."
        //}
        [JsonProperty("response_type")]
        public string ResponseType { get; set; }

        public string Text { get; set; }
    }
}
