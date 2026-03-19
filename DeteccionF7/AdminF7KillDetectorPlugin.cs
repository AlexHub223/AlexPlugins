using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Unturned.Events;
using Rocket.API.Collections;
using SDG.Unturned;
using Steamworks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace AdminF7KillDetector
{
    public class AdminF7KillDetectorPlugin : RocketPlugin<AdminF7KillDetectorConfiguration>
    {
        public static AdminF7KillDetectorPlugin Instance { get; private set; }
        
        private Dictionary<ulong, bool> spectatorActive = new Dictionary<ulong, bool>();
        
        private Dictionary<ulong, F7KillInfo> pendingScreenshots = new Dictionary<ulong, F7KillInfo>();

        private static readonly Regex F7LogRegex = new Regex(@"(\d+)\[\d+\] "".*"" turned (on|off) spectator stats overlay admin mode", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private string logFilePath;

        private class F7KillInfo
        {
            public UnturnedPlayer Admin { get; set; }
            public UnturnedPlayer Victim { get; set; }
            public string LogMessage { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private Harmony harmony;

        protected override void Load()
        {
            Instance = this;
            logFilePath = Path.Combine(Application.dataPath, "Logs", "AdminSpyLogger.txt");

            string logDir = Path.GetDirectoryName(logFilePath);
            if (!System.IO.Directory.Exists(logDir)) System.IO.Directory.CreateDirectory(logDir);


            

            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;


            Player.OnAnyPlayerAdminUsageChanged += OnAdminUsageChanged;
            
            try
            {
                harmony = new Harmony("com.adminf7killdetector.adminspy");
                
                var playerType = typeof(SDG.Unturned.Player);
                var targetMethod = playerType.GetMethod("HandleScreenshotData", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (targetMethod == null)
                {
                    Logger.LogWarning("[AdminF7KillDetector] 'HandleScreenshotData' not found. Searching for alternatives...");
                    var alternatives = playerType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static)
                        .Where(m => m.Name.ToLower().Contains("screenshot"))
                        .ToList();
                    
                    targetMethod = alternatives.FirstOrDefault(m => 
                        m.Name.Equals("HandleScreenshotData", StringComparison.OrdinalIgnoreCase) || 
                        m.Name.Equals("ReceiveScreenshotRelay", StringComparison.OrdinalIgnoreCase) ||
                        m.Name.Equals("receiveScreenshot", StringComparison.OrdinalIgnoreCase));
                }
 
                if (targetMethod != null)
                {
                    Logger.Log($"[AdminF7KillDetector] Patching target: {targetMethod.DeclaringType.FullName}.{targetMethod.Name}");
                    var prefix = typeof(ScreenshotPatch).GetMethod("Prefix", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    harmony.Patch(targetMethod, new HarmonyMethod(prefix));
                    Logger.Log("[AdminF7KillDetector] Harmony patch applied successfully!");
                }
                else
                {
                    Logger.LogError("[AdminF7KillDetector] CRITICAL: Could not find any screenshot receiving method to patch. /spy features will be disabled.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error applying manual Harmony patch");
            }

            Logger.Log($"{Name} {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} has been loaded!");
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
            Player.OnAnyPlayerAdminUsageChanged -= OnAdminUsageChanged;
            
            if (harmony != null) harmony.UnpatchAll("com.adminf7killdetector.adminspy");

            Logger.Log($"{Name} has been unloaded!");
        }

        private void OnAdminUsageChanged(Player player, EPlayerAdminUsageFlags oldFlags, EPlayerAdminUsageFlags newFlags)
        {
            try
            {
                if (player == null || player.channel == null) return;

                ulong steamId = player.channel.owner.playerID.steamID.m_SteamID;
                bool isF7Active = (newFlags & EPlayerAdminUsageFlags.SpectatorStatsOverlay) != 0;

                spectatorActive[steamId] = isF7Active;
                Logger.Log($"[AdminF7KillDetector] EVENT: {player.channel.owner.playerID.characterName} ({steamId}) F7 is now {(isF7Active ? "ACTIVE" : "INACTIVE")}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error in OnAdminUsageChanged handler");
            }
        }

        private void OnPlayerDeath(UnturnedPlayer victim, EDeathCause cause, ELimb limb, CSteamID instigator)
        {
            try
            {
                if (instigator == CSteamID.Nil || instigator == victim.CSteamID) return;

                UnturnedPlayer killer = UnturnedPlayer.FromCSteamID(instigator);
                if (killer == null) return;

                ulong killerSid = instigator.m_SteamID;
                
                bool isF7Active = false;
                try
                {
                    isF7Active = (killer.Player.AdminUsageFlags & EPlayerAdminUsageFlags.SpectatorStatsOverlay) != 0;
                }
                catch { }

                if (!isF7Active)
                {
                    spectatorActive.TryGetValue(killerSid, out isF7Active);
                }

                Logger.Log($"[AdminF7KillDetector] DEBUG: {killerSid} killed {victim.CharacterName}. F7 Status: {isF7Active}");

                if (isF7Active)
                {
                    Logger.Log($"[AdminF7KillDetector] TRIGGERED: {killerSid} killed {victim.CharacterName} while in F7 mode!");
                    HandleF7Kill(instigator, victim);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error in OnPlayerDeath handling");
            }
        }

        private void HandleF7Kill(CSteamID adminSid, UnturnedPlayer victim)
        {
            UnturnedPlayer admin = UnturnedPlayer.FromCSteamID(adminSid);
            string adminName = admin?.CharacterName ?? "Unknown";
            string victimName = victim?.CharacterName ?? "Unknown";
            string message = $"[AdminSpyLogger] WARN: {adminName} ({adminSid}) killed {victimName} while spectator overlay was ACTIVE";

            Logger.LogWarning(message);

            if (Configuration.Instance.LogToFile)
            {
                try
                {
                    File.AppendAllText(logFilePath, $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] {message}{System.Environment.NewLine}");
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Failed to write to local log file");
                }
            }

            if (admin != null)
            {
                var info = new F7KillInfo
                {
                    Admin = admin,
                    Victim = victim,
                    LogMessage = message,
                    Timestamp = DateTime.UtcNow
                };

                pendingScreenshots[adminSid.m_SteamID] = info;

                Task.Run(async () => await SendToDiscordTextOnly(info));

                admin.Player.sendScreenshot(CSteamID.Nil);
            }
        }

        private async Task SendToDiscordTextOnly(F7KillInfo info)
        {
            if (string.IsNullOrEmpty(Configuration.Instance.DiscordWebhookUrl)) return;

            var payload = new
            {
                username = Configuration.Instance.WebhookUsername,
                avatar_url = Configuration.Instance.WebhookAvatarUrl,
                embeds = new[]
                {
                    new
                    {
                        title = "🚨 F7 Abuse Detected (Pending Spy) 🚨",
                        description = info.LogMessage,
                        color = 16753920, // Orange
                        fields = new[]
                        {
                            new { name = "Admin", value = $"{info.Admin.CharacterName} ({info.Admin.CSteamID})", inline = true },
                            new { name = "Victim", value = $"{info.Victim.CharacterName} ({info.Victim.CSteamID})", inline = true }
                        },
                        timestamp = info.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    }
                }
            };

            await WebhookHelper.SendWebhook(Configuration.Instance.DiscordWebhookUrl, payload);
        }

        private Task SendToDiscord(object payload)
        {
            return Task.CompletedTask;
        }

        public void InternalOnScreenshotRelayed(CSteamID steamID, byte[] compressed)
        {
            try
            {
                if (compressed == null || compressed.Length == 0) return;

                ulong sid = steamID.m_SteamID;
                if (pendingScreenshots.TryGetValue(sid, out F7KillInfo info))
                {
                    pendingScreenshots.Remove(sid);
                    
                    Task.Run(async () => await SendToDiscordWithImage(info, compressed));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error processing internal screenshot relay");
            }
        }

        private async Task SendToDiscordWithImage(F7KillInfo info, byte[] image)
        {
            if (string.IsNullOrEmpty(Configuration.Instance.DiscordWebhookUrl)) return;

            var payload = new
            {
                username = Configuration.Instance.WebhookUsername,
                avatar_url = Configuration.Instance.WebhookAvatarUrl,
                embeds = new[]
                {
                    new
                    {
                        title = "🚨 F7 Abuse Detected 🚨",
                        description = info.LogMessage,
                        color = 16711680, // Red
                        fields = new[]
                        {
                            new { name = "Admin", value = $"{info.Admin.CharacterName} ({info.Admin.CSteamID})", inline = true },
                            new { name = "Victim", value = $"{info.Victim.CharacterName} ({info.Victim.CSteamID})", inline = true },
                            new { name = "Location", value = info.Admin.Position.ToString(), inline = false }
                        },
                        image = new { url = "attachment://screenshot.jpg" },
                        timestamp = info.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    }
                }
            };

            await WebhookHelper.SendWebhookWithImage(Configuration.Instance.DiscordWebhookUrl, payload, image);
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "f7_abuse_warning", "[AdminSpyLogger] WARN: {0} killed {1} while using F7!" }
        };
    }

    public static class ScreenshotPatch
    {
        public static void Prefix(object[] __args, object __instance)
        {
            try
            {
                if (__instance == null || AdminF7KillDetectorPlugin.Instance == null) return;

                byte[] compressed = null;
                CSteamID steamID = CSteamID.Nil;

                if (__args != null && __args.Length > 0 && __args[0] is byte[] bytes)
                {
                    compressed = bytes;
                }

                if (compressed != null)
                {
                    if (__instance is SDG.Unturned.Player player && player.channel != null)
                    {
                        steamID = player.channel.owner.playerID.steamID;
                    }

                    if (steamID != CSteamID.Nil)
                    {
                        AdminF7KillDetectorPlugin.Instance.InternalOnScreenshotRelayed(steamID, compressed);
                    }
                }
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogException(ex, "Error in dynamic Screenshot Prefix");
            }
        }
    }
}
