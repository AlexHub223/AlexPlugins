using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminF7KillDetector
{
    public class AdminF7KillDetectorConfiguration : IRocketPluginConfiguration
    {
        public bool LogToFile { get; set; }
        public bool BroadcastToAdmins { get; set; }
        public string DiscordWebhookUrl { get; set; }
        public string WebhookUsername { get; set; }
        public string WebhookAvatarUrl { get; set; }

        public void LoadDefaults()
        {
            LogToFile = true;
            BroadcastToAdmins = true;
            DiscordWebhookUrl = "";
            WebhookUsername = "Admin Spy Logger";
            WebhookAvatarUrl = "https://i.imgur.com/8n9as77.png";
        }
    }
}
