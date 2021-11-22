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
using System.Security.Cryptography;
using System.Text;

namespace Quote
{
    public static class Quote
    {
        [FunctionName("Quote")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string timestamp = req.Headers["X-Slack-Request-Timestamp"];
            string signature = req.Headers["X-Slack-Signature"];
            string body = await req.ReadAsStringAsync();
            string validation = $"v0:{timestamp}:{body}";

            log.LogInformation(validation);

            string signingSecret = Environment.GetEnvironmentVariable("SlackSigningSecret", EnvironmentVariableTarget.Process);
            UTF8Encoding encoder = new UTF8Encoding();
            HMACSHA256 sha256 = new HMACSHA256(encoder.GetBytes(signingSecret));
            byte[] hash = sha256.ComputeHash(encoder.GetBytes(validation));
            string hex = String.Concat("v0=", ToHex(hash, false));

            if (hex != signature)
            {
                log.LogInformation($"Validation failed - Hex: {hex}");
                log.LogInformation($"Validation failed - Signature: {signature}");
                return new UnauthorizedResult();
            }

            IList<string> quotes = new List<string>() {
                "Nothing is impossible. The word itself says 'I’m possible!' - Audrey Hepburn",
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

        private static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
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
