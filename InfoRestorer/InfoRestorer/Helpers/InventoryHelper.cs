using Adam.InfoRestorer.Models;
using SDG.Unturned;
using System.Collections.Generic;

namespace Adam.InfoRestorer.Helpers
{
    public class InventoryHelper
    {
        public static readonly byte[] EMPTY_BYTE_ARRAY = new byte[0];

        // copied from uEssentials :P
        public static void ClearPlayerInventory(Player player)
        {
            var playerInv = player.inventory;

            // Remove items
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                if (page == PlayerInventory.AREA || page == PlayerInventory.STORAGE)
                    continue;

                var count = playerInv.getItemCount(page);

                for (byte index = 0; index < count; index++)
                {
                    playerInv.removeItem(page, 0);
                }
            }

            // Remove clothes

            // Remove unequipped cloths
            System.Action removeUnequipped = () => {
                for (byte i = 0; i < playerInv.getItemCount(2); i++)
                {
                    playerInv.removeItem(2, 0);
                }
            };

            // Unequip & remove from inventory
            player.clothing.askWearBackpack(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.clothing.askWearGlasses(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.clothing.askWearHat(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.clothing.askWearPants(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.clothing.askWearMask(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.clothing.askWearShirt(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();

            player.clothing.askWearVest(0, 0, EMPTY_BYTE_ARRAY, true);
            removeUnequipped();
        }

        public static void GetInventoryItems(Player player, List<ItemModel> items)
        {
            var clothing = player.clothing;

            if (clothing.backpack != 0)
                items.Add(ItemModel.FromClothing(clothing.backpack, clothing.backpackQuality, clothing.backpackState));
            if (clothing.vest != 0)
                items.Add(ItemModel.FromClothing(clothing.vest, clothing.vestQuality, clothing.vestState));
            if (clothing.shirt != 0)
                items.Add(ItemModel.FromClothing(clothing.shirt, clothing.shirtQuality, clothing.shirtState));
            if (clothing.pants != 0)
                items.Add(ItemModel.FromClothing(clothing.pants, clothing.pantsQuality, clothing.pantsState));
            if (clothing.mask != 0)
                items.Add(ItemModel.FromClothing(clothing.mask, clothing.maskQuality, clothing.maskState));
            if (clothing.hat != 0)
                items.Add(ItemModel.FromClothing(clothing.hat, clothing.hatQuality, clothing.hatState));
            if (clothing.glasses != 0)
                items.Add(ItemModel.FromClothing(clothing.glasses, clothing.glassesQuality, clothing.glassesState));

            for (byte page = 0; page < PlayerInventory.PAGES - 2; page++)
            {
                for (byte index = 0; index < player.inventory.getItemCount(page); index++)
                {
                    var jar = player.inventory.getItem(page, index);
                    if (jar == null)
                        continue;

                    items.Add(ItemModel.FromItemJar(jar, page));
                }
            }
        }

        public static void GiveInventoryItems(Player player, List<ItemModel> items)
        {
            foreach (var item in items)
            {
                if (item.Page == byte.MaxValue)
                {
                    player.inventory.forceAddItem(item.Item, true);
                    continue;
                }

                if (!player.inventory.tryAddItem(item.Item, item.X, item.Y, item.Page, item.Rot))
                {
                    player.inventory.forceAddItem(item.Item, false);
                }
            }
        }
    }
}
