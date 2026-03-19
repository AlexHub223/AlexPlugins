using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Linq;
using UnityEngine;

namespace BlueCrystalCooking.functions
{
    public static class MethBagFunctions
    {
        public static void OnGestureChanged(UnturnedPlayer player, EPlayerGesture gesture)
        {
            if (player == null) return;

            if (Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward, out RaycastHit raycastHit, 2, RayMasks.BARRICADE))
            {
                BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(raycastHit.transform);
                if (drop != null && drop.asset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.FrozenTrayId)
                {
                    int amount = UnityEngine.Random.Range(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalBagsAmountMin, BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalBagsAmountMax + 1);
                    
                    for (int i = 0; i < amount; i++)
                    {
                        ItemManager.dropItem(new Item(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalBagId, true), raycastHit.transform.position + Vector3.up, false, true, true);
                    }

                    if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBlueCrystalFreezeEffect)
                    {
                        EffectManager.sendEffect(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalFreezeEffectId, 10, raycastHit.transform.position);
                    }

                    ChatManager.serverSendMessage(BlueCrystalCookingPlugin.Instance.Translate("bluecrystalbags_obtained", amount), Color.white, null, player.SteamPlayer(), EChatMode.SAY, BlueCrystalCookingPlugin.Instance.Configuration.Instance.IconImageUrl, true);
                    
                    if (BarricadeManager.tryGetInfo(drop.model, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region))
                    {
                        BarricadeManager.destroyBarricade(region, x, y, plant, index);
                    }
                }
            }
        }

        public static void ConsumeAction(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            if (consumeableAsset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalBagId)
            {
                UnturnedPlayer player = UnturnedPlayer.FromPlayer(instigatingPlayer);
                BlueCrystalCookingPlugin.Instance.drugeffectPlayersList.Add(new DrugeffectTimeObject(player.Id));
                
                ApplyDrugEffects(player);
            }
        }

        public static void Update()
        {
            if (BlueCrystalCookingPlugin.Instance.drugeffectPlayersList.Count == 0) return;

            long currentTime = BlueCrystalCookingPlugin.getCurrentTime();
            foreach (var drugeffect in BlueCrystalCookingPlugin.Instance.drugeffectPlayersList.ToList())
            {
                if (currentTime - drugeffect.time >= BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugEffectDurationSecs)
                {
                    BlueCrystalCookingPlugin.Instance.drugeffectPlayersList.Remove(drugeffect);
                    UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(ulong.Parse(drugeffect.playerId)));
                    if (player != null)
                    {
                        ApplyDrugEffects(player);
                    }
                }
            }
        }

        private static void ApplyDrugEffects(UnturnedPlayer player)
        {
            bool hasEffect = BlueCrystalCookingPlugin.Instance.drugeffectPlayersList.Any(d => d.playerId == player.Id);
            
            float speedMult = 1f;
            float jumpMult = 1f;

            if (hasEffect)
            {
                if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.UseDrugEffectSpeed)
                    speedMult = BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugEffectSpeedMultiplier;
                
                if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.UseDrugEffectJump)
                    jumpMult = BlueCrystalCookingPlugin.Instance.Configuration.Instance.DrugEffectJumpMultiplier;
            }

            player.Player.movement.sendPluginSpeedMultiplier(speedMult);
            player.Player.movement.sendPluginJumpMultiplier(jumpMult);
        }
    }
}
