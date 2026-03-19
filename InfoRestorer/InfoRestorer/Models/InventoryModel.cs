using Adam.InfoRestorer.Helpers;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adam.InfoRestorer.Models
{
    public class InventoryModel
    {
        public CSteamID SteamID { get; set; }
        public List<ItemModel> Items { get; set; }

        public static InventoryModel FromPlayer(Player player)
        {
            var inventory = new InventoryModel() 
            { 
                SteamID = player.channel.owner.playerID.steamID, 
                Items = new List<ItemModel>() 
            };
            InventoryHelper.GetInventoryItems(player, inventory.Items);
            return inventory;
        }

        public void LoadToPlayer(Player player, bool shouldClear)
        {
            if (shouldClear)
                InventoryHelper.ClearPlayerInventory(player);
            InventoryHelper.GiveInventoryItems(player, Items);
        }
    }
}
