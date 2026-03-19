using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rocket.Core.Logging;
using SDG.Unturned;
using System.Linq;

namespace Ocelot.BlueCrystalCooking.Utils
{
    public class LicenseManager
    {
        private const string GistUrl = "https://gist.githubusercontent.com/Hologts27/d2687693b1d948bc77de884e3165b366/raw/whitelist.json";
        private const string IpApiUrl = "https://api.ipify.org";

        public class WhitelistData
        {
            public bool Enabled { get; set; }
            public string Message { get; set; }
            public Dictionary<string, List<string>> Whitelists { get; set; }
        }

        public static async Task<bool> CheckLicense(string serverIp, ushort serverPort)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "BlueCrystalCookingLoader");

                    // 1. Get the REAL Public IP
                    string publicIp = serverIp;
                    try 
                    {
                        publicIp = (await client.GetStringAsync(IpApiUrl)).Trim();
                    }
                    catch 
                    {
                        Logger.LogWarning("[Loader] Could not fetch public IP, falling back to Provider IP.");
                    }
                    
                    // 2. Fetch the Whitelist (with cache buster)
                    string cacheBuster = Guid.NewGuid().ToString();
                    string finalUrl = $"{GistUrl}?t={cacheBuster}";
                    
                    string json = await client.GetStringAsync(finalUrl);
                    var data = JsonConvert.DeserializeObject<WhitelistData>(json);

                    if (data == null || !data.Enabled)
                    {
                        Logger.LogError("[Loader] License system is currently disabled or Gist is invalid.");
                        return false;
                    }

                    // 3. Check against the whitelist (Trimming everything to be safe)
                    string currentServer = $"{publicIp}:{serverPort}".Trim();
                    
                    // Log for debugging
                    Logger.Log($"[Loader] Checking license for: {currentServer}");

                    if (data.Whitelists != null && data.Whitelists.TryGetValue("BlueCrystalCooking", out List<string> whitelist))
                    {
                        Logger.Log($"[Loader] Found {whitelist?.Count ?? 0} whitelisted servers for BlueCrystalCooking.");
                        
                        if (whitelist != null && whitelist.Any(s => s.Trim().Equals(currentServer, StringComparison.OrdinalIgnoreCase)))
                        {
                            Logger.Log($"[Loader] License verified for {currentServer}!");
                            return true;
                        }
                    }
                    else
                    {
                        Logger.LogError("[Loader] Plugin 'BlueCrystalCooking' not found in whitelist database.");
                    }

                    Logger.LogError($"[Loader] {data.Message}");
                    Logger.LogError($"[Loader] Attempted IP: {currentServer}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[Loader] Failed to verify license: {ex.Message}");
                return false;
            }
        }
    }
}
