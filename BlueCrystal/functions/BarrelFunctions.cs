using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlueCrystalCooking.functions
{
    public static class BarrelFunctions
    {
        public static void OnGestureChanged(UnturnedPlayer player, EPlayerGesture gesture)
        {
            if (player == null) return;

            if (Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward, out RaycastHit raycastHit, 2, RayMasks.BARRICADE))
            {
                BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(raycastHit.transform);
                if (drop == null) return;

                if (drop.asset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId)
                {
                    if (BlueCrystalCookingPlugin.Instance.placedBarrelsIngredients.TryGetValue(drop.instanceID, out BarrelObject barrel))
                    {
                        bool allPresent = BlueCrystalCookingPlugin.Instance.Configuration.Instance.drugIngredientIds.All(id => barrel.ingredients.Contains(id));

                        if (allPresent)
                        {
                            if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBarrelStirEffect)
                            {
                                EffectManager.sendEffect(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelStirEffectId, 4, raycastHit.transform.position);
                            }
                            barrel.progress += BlueCrystalCookingPlugin.Instance.Configuration.Instance.StirProgressAddPercentage;
                            
                            if (barrel.progress >= 100)
                            {
                                barrel.progress = 0;
                                foreach (ushort id in BlueCrystalCookingPlugin.Instance.Configuration.Instance.drugIngredientIds)
                                {
                                    barrel.ingredients.Remove(id);
                                }

                                ChatManager.serverSendMessage(BlueCrystalCookingPlugin.Instance.Translate("stir_successful"), Color.white, null, player.SteamPlayer(), EChatMode.SAY, BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                                
                                ItemBarricadeAsset trayAsset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayId);
                                if (trayAsset != null)
                                {
                                    BarricadeManager.dropBarricade(new Barricade(trayAsset), null, player.Position, 0, 0, 0, (ulong)player.CSteamID, (ulong)player.Player.quests.groupID);
                                }
                            }
                        }
                        else
                        {
                            ChatManager.serverSendMessage(BlueCrystalCookingPlugin.Instance.Translate("not_enough_ingredients"), Color.white, null, player.SteamPlayer(), EChatMode.SAY, BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                        }
                    }
                }
            }
        }

        public static void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            Vector3 targetPos = point;

            if (asset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId)
            {
                BlueCrystalCookingPlugin.Instance.Wait(0.5f, () => {
                    FindAndRegisterBarrel(targetPos);
                });
            }

            if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.drugIngredientIds.Contains(asset.id))
            {
                BarricadeDrop barrelDrop = FindNearbyBarricade(targetPos, BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId, 1.5f);
                
                if (barrelDrop != null)
                {
                    if (!BlueCrystalCookingPlugin.Instance.placedBarrelsIngredients.TryGetValue(barrelDrop.instanceID, out BarrelObject barrel))
                    {
                        barrel = new BarrelObject(new List<ushort>(), 0);
                        BlueCrystalCookingPlugin.Instance.placedBarrelsIngredients.Add(barrelDrop.instanceID, barrel);
                    }

                    barrel.ingredients.Add(asset.id);
                    
                    SteamPlayer sPlayer = PlayerTool.getSteamPlayer(owner);
                    if (sPlayer != null)
                    {
                        ChatManager.serverSendMessage(BlueCrystalCookingPlugin.Instance.Translate("ingredient_added", asset.itemName), Color.white, null, sPlayer, EChatMode.SAY, BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                    }

                    BlueCrystalCookingPlugin.Instance.Wait(0.5f, () => {
                        BarricadeDrop ingredientDrop = FindNearbyBarricade(targetPos, asset.id, 0.5f);
                        if (ingredientDrop != null)
                        {
                            if (BarricadeManager.tryGetInfo(ingredientDrop.model, out byte ix, out byte iy, out ushort iplant, out ushort iindex, out BarricadeRegion iregion))
                            {
                                BarricadeManager.destroyBarricade(iregion, ix, iy, iplant, iindex);
                            }
                        }
                    });
                }
            }
        }

        private static void FindAndRegisterBarrel(Vector3 pos)
        {
            BarricadeDrop barrel = FindNearbyBarricade(pos, BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId, 0.5f);
            if (barrel != null)
            {
                if (!BlueCrystalCookingPlugin.Instance.placedBarrelsIngredients.ContainsKey(barrel.instanceID))
                {
                    BlueCrystalCookingPlugin.Instance.placedBarrelsIngredients.Add(barrel.instanceID, new BarrelObject(new List<ushort>(), 0));
                }
            }
        }

        public static BarricadeDrop FindNearbyBarricade(Vector3 pos, ushort id, float radius)
        {
            float sqrRadius = radius * radius;
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops)
                {
                    if (drop.asset.id == id && (drop.model.position - pos).sqrMagnitude <= sqrRadius)
                    {
                        return drop;
                    }
                }
            }
            foreach (var region in BarricadeManager.vehicleRegions)
            {
                foreach (var drop in region.drops)
                {
                    if (drop.asset.id == id && (drop.model.position - pos).sqrMagnitude <= sqrRadius)
                    {
                        return drop;
                    }
                }
            }
            return null;
        }
    }
}
