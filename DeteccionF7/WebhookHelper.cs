using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AdminF7KillDetector
{
    public static class WebhookHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task SendWebhook(string url, object payload)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogException(ex, "Failed to send plain Discord webhook");
            }
        }

        public static async Task SendWebhookWithImage(string url, object payload, byte[] imageData, string fileName = "screenshot.jpg")
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                using (var content = new MultipartFormDataContent())
                {
                    var json = JsonConvert.SerializeObject(payload);
                    content.Add(new StringContent(json, Encoding.UTF8, "application/json"), "payload_json");

                    if (imageData != null && imageData.Length > 0)
                    {
                        var imageContent = new ByteArrayContent(imageData);
                        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                        content.Add(imageContent, "file", fileName);
                    }

                    await client.PostAsync(url, content);
                }
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogException(ex, "Failed to send Discord webhook with image");
            }
        }
    }
}
