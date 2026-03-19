# AdminF7KillDetector

**AdminF7KillDetector** is an Unturned RocketMod plugin designed to monitor and log admin activity, specifically focusing on the use of the F7 spectator overlay during combat. It helps server owners ensure fair play by detecting when an admin kills a player with the overlay active and automatically capturing evidence.

## 🚀 Features

- **F7 Abuse Detection**: Automatically detects when a player is killed by an admin who has the F7 spectator stats overlay enabled.
- **Discord Integration**: Sends real-time alerts to a Discord channel via Webhooks.
- **Auto-Screenshot**: Automatically requests and sends a screenshot from the admin's perspective to Discord when a violation is detected.
- **Harmony Patching**: Uses Harmony to intercept screenshot data for reliable evidence gathering.
- **Local Logging**: Maintains a local text log of all detected incidents.
- **Customizable**: Fully configurable webhook settings and logging options.

## 📦 Installation

1. Download the latest release `.dll`.
2. Place the `AdminF7KillDetector.dll` into your server's `Plugins` folder.
3. Start the server to generate the configuration file.
4. Edit the configuration file located in `Plugins/AdminF7KillDetector/AdminF7KillDetector.configuration.xml`.
5. Restart the server or reload the plugin.

## ⚙️ Configuration

```xml
<AdminF7KillDetectorConfiguration>
  <LogToFile>true</LogToFile>
  <BroadcastToAdmins>true</BroadcastToAdmins>
  <DiscordWebhookUrl>YOUR_WEBHOOK_URL</DiscordWebhookUrl>
  <WebhookUsername>Admin F7 Detector</WebhookUsername>
  <WebhookAvatarUrl>https://i.imgur.com/8n9as77.png</WebhookAvatarUrl>
</AdminF7KillDetectorConfiguration>
```

- **LogToFile**: Whether to save detections to a local `.txt` file.
- **BroadcastToAdmins**: (Reserved for future use/broadcast alerts).
- **DiscordWebhookUrl**: Your Discord channel's webhook URL.
- **WebhookUsername**: The name displayed by the bot in Discord.
- **WebhookAvatarUrl**: The avatar image for the Discord bot.

## 🛠️ Requirements

- [RocketMod](https://rocketmod.net/) for Unturned.
- [Lib.Harmony](https://github.com/pardeike/Harmony) (included in modern RocketMod distributions).

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
