using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace InfoRestorer
{
    public class InfoRestorerPlugin : RocketPlugin<InfoRestorerConfiguration>
    {
        public static InfoRestorerPlugin Instance { get; private set; }
        public Color MessageColor { get; set; }
        public Dictionary<CSteamID, List<InventoryBackup>> Backups { get; private set; }

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, Color.green);
            Backups = new Dictionary<CSteamID, List<InventoryBackup>>();

            PlayerLife.OnPreDeath += OnPreDeath;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            U.Events.OnPlayerConnected += OnPlayerConnected;

            Logger.Log($"{Name} {Assembly.GetName().Version} by Alex has been loaded!", ConsoleColor.Cyan);
        }

        protected override void Unload()
        {
            PlayerLife.OnPreDeath -= OnPreDeath;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            U.Events.OnPlayerConnected -= OnPlayerConnected;

            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Cyan);
        }

        private void OnPreDeath(PlayerLife life)
        {
            var steamId = life.player.channel.owner.playerID.steamID;
            if (!Backups.ContainsKey(steamId))
                Backups[steamId] = new List<InventoryBackup>();

            var backup = InventoryBackup.FromPlayer(life.player);
            Backups[steamId].Insert(0, backup);

            if (Backups[steamId].Count > Configuration.Instance.MaxSavesPerPlayer)
                Backups[steamId].RemoveAt(Backups[steamId].Count - 1);
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            if (!Backups.ContainsKey(player.CSteamID))
                Backups.Add(player.CSteamID, new List<InventoryBackup>());
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            if (Configuration.Instance.ShouldRemoveSavesOnLeave)
                Backups.Remove(player.CSteamID);
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "usage", "Invalid Syntax! Usage: /restore <player> [version]" },
            { "player_not_found", "Player not found!" },
            { "not_a_number", "{0} is not a valid number!" },
            { "no_backups", "{0} doesn't have any backups available!" },
            { "out_of_range", "{0} only has {1} backups!" },
            { "restored", "Successfully restored {0}'s inventory (Version: {1})!" }
        };
    }

    public class InventoryBackup
    {
        public List<ItemData> Clothing { get; set; }
        public List<ItemData> Inventory { get; set; }
        public DateTime Timestamp { get; set; }

        public static InventoryBackup FromPlayer(Player player)
        {
            var backup = new InventoryBackup
            {
                Timestamp = DateTime.UtcNow,
                Clothing = new List<ItemData>(),
                Inventory = new List<ItemData>()
            };

            var clothing = player.clothing;
            if (clothing.backpack != 0) backup.Clothing.Add(new ItemData(clothing.backpack, clothing.backpackQuality, clothing.backpackState));
            if (clothing.vest != 0) backup.Clothing.Add(new ItemData(clothing.vest, clothing.vestQuality, clothing.vestState));
            if (clothing.shirt != 0) backup.Clothing.Add(new ItemData(clothing.shirt, clothing.shirtQuality, clothing.shirtState));
            if (clothing.pants != 0) backup.Clothing.Add(new ItemData(clothing.pants, clothing.pantsQuality, clothing.pantsState));
            if (clothing.mask != 0) backup.Clothing.Add(new ItemData(clothing.mask, clothing.maskQuality, clothing.maskState));
            if (clothing.hat != 0) backup.Clothing.Add(new ItemData(clothing.hat, clothing.hatQuality, clothing.hatState));
            if (clothing.glasses != 0) backup.Clothing.Add(new ItemData(clothing.glasses, clothing.glassesQuality, clothing.glassesState));

            for (byte page = 0; page < PlayerInventory.PAGES - 2; page++)
            {
                var itemCount = player.inventory.getItemCount(page);
                for (byte index = 0; index < itemCount; index++)
                {
                    var jar = player.inventory.getItem(page, index);
                    if (jar != null) backup.Inventory.Add(new ItemData(jar, page));
                }
            }

            return backup;
        }

        public void ApplyTo(Player player, bool clearExisting)
        {
            if (clearExisting) ClearInventory(player);

            foreach (var item in Clothing) player.inventory.forceAddItem(item.ToItem(), true);
            foreach (var item in Inventory)
            {
                if (!player.inventory.tryAddItem(item.ToItem(), item.X, item.Y, item.Page, item.Rot))
                    player.inventory.forceAddItem(item.ToItem(), false);
            }
        }

        private void ClearInventory(Player player)
        {
            var inv = player.inventory;
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                if (page == PlayerInventory.AREA || page == PlayerInventory.STORAGE) continue;
                while (inv.getItemCount(page) > 0) inv.removeItem(page, 0);
            }

            var emptyState = new byte[0];
            player.clothing.askWearBackpack(0, 0, emptyState, true);
            player.clothing.askWearVest(0, 0, emptyState, true);
            player.clothing.askWearShirt(0, 0, emptyState, true);
            player.clothing.askWearPants(0, 0, emptyState, true);
            player.clothing.askWearMask(0, 0, emptyState, true);
            player.clothing.askWearHat(0, 0, emptyState, true);
            player.clothing.askWearGlasses(0, 0, emptyState, true);
            
            // Re-clear to ensure dropped items when unwear are gone
            for (byte page = 0; page < 7; page++)
            {
                while (inv.getItemCount(page) > 0) inv.removeItem(page, 0);
            }
        }
    }

    public class ItemData
    {
        public ushort ItemId { get; set; }
        public byte Quality { get; set; }
        public byte[] State { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Rot { get; set; }
        public byte Page { get; set; }

        public ItemData() { }

        public ItemData(ushort id, byte quality, byte[] state)
        {
            ItemId = id;
            Quality = quality;
            State = state;
            Page = byte.MaxValue;
        }

        public ItemData(ItemJar jar, byte page)
        {
            ItemId = jar.item.id;
            Quality = jar.item.quality;
            State = jar.item.state;
            X = jar.x;
            Y = jar.y;
            Rot = jar.rot;
            Page = page;
        }

        public Item ToItem() => new Item(ItemId, 1, Quality, State);
    }
}
