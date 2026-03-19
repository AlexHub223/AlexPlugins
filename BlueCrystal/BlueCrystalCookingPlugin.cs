using JetBrains.Annotations;
using BlueCrystalCooking.functions;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace BlueCrystalCooking
{
    public class BlueCrystalCookingPlugin : RocketPlugin<BlueCrystalCookingConfiguration>
    {
        public static BlueCrystalCookingPlugin Instance;
        public const string VERSION = "1.2.0";

        private int Frame = 0;
        public long timer = 0;


        public Dictionary<uint, BarrelObject> placedBarrelsIngredients = new Dictionary<uint, BarrelObject>();
        public List<DrugeffectTimeObject> drugeffectPlayersList = new List<DrugeffectTimeObject>();
        public List<FreezingTrayObject> freezingTrays = new List<FreezingTrayObject>();

        protected override void Load()
        {
            Instance = this;
            Logger.Log("BlueCrystalCookingPlugin v" + VERSION + " (Fixed for modern Unturned) loaded!", ConsoleColor.Yellow);

            BarricadeManager.onDeployBarricadeRequested += BarricadeDeployed;
            BarricadeDrop.OnSalvageRequested_Global += OnSalvageRequested;
            PlayerAnimator.OnGestureChanged_Global += OnGestureChanged;
            UseableConsumeable.onConsumePerformed += ConsumeAction;
            BarricadeManager.onDamageBarricadeRequested += BarricadeDamaged;

            if (Level.isLoaded)
            {
                AddExistingBarrels(0);
            }
            else
            {
                Level.onLevelLoaded += AddExistingBarrels;
            }
        }

        protected override void Unload()
        {
            BarricadeManager.onDeployBarricadeRequested -= BarricadeDeployed;
            BarricadeDrop.OnSalvageRequested_Global -= OnSalvageRequested;
            PlayerAnimator.OnGestureChanged_Global -= OnGestureChanged;
            UseableConsumeable.onConsumePerformed -= ConsumeAction;
            BarricadeManager.onDamageBarricadeRequested -= BarricadeDamaged;
            Level.onLevelLoaded -= AddExistingBarrels;

            Instance = null;
        }

        private void OnSalvageRequested(BarricadeDrop drop, SteamPlayer salvager, ref bool shouldAllow)
        {
            if (drop.asset.id == Configuration.Instance.BarrelObjectId)
            {
                placedBarrelsIngredients.Remove(drop.instanceID);
            }
            else if (drop.asset.id == Configuration.Instance.BlueCrystalTrayId || drop.asset.id == Configuration.Instance.LiquidTrayId)
            {
                freezingTrays.RemoveAll(t => t.instanceID == drop.instanceID);
            }
        }

        private void ConsumeAction(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            MethBagFunctions.ConsumeAction(instigatingPlayer, consumeableAsset);
        }

        private void OnGestureChanged(PlayerAnimator animator, EPlayerGesture gesture)
        {
            if (gesture == EPlayerGesture.PUNCH_LEFT || gesture == EPlayerGesture.PUNCH_RIGHT)
            {
                BarrelFunctions.OnGestureChanged(UnturnedPlayer.FromPlayer(animator.player), gesture);
                MethBagFunctions.OnGestureChanged(UnturnedPlayer.FromPlayer(animator.player), gesture);
            }
        }

        private void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            BarrelFunctions.BarricadeDeployed(barricade, asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
            FreezerFunctions.BarricadeDeployed(barricade, asset, hit, ref point, ref angle_x, ref angle_y, ref angle_z, ref owner, ref group, ref shouldAllow);
        }

        private void BarricadeDamaged(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            BarricadeFunctions.BarricadeDamaged(barricadeTransform, pendingTotalDamage);
        }

        private void AddExistingBarrels(int level)
        {
            Logger.Log("Adding map barrels to list...", ConsoleColor.Green);
            int count = 0;
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops)
                {
                    if (drop.asset.id == Configuration.Instance.BarrelObjectId)
                    {
                        if (!placedBarrelsIngredients.ContainsKey(drop.instanceID))
                        {
                            placedBarrelsIngredients.Add(drop.instanceID, new BarrelObject(new List<ushort>(), 0));
                            count++;
                        }
                    }
                }
            }
            Logger.Log($"{count} barrels added.", ConsoleColor.Green);
        }

        public override TranslationList DefaultTranslations => new TranslationList
        {
            {"not_enough_ingredients", "There are <color=#ff3c19>not enough ingredients</color> in the barrel to stir them into blue crystal." },
            {"ingredient_added", "You have <color=#75ff19>added {0}</color> to the barrel." },
            {"stir_successful", "You have <color=#75ff19>successfully mixed</color> the ingredients into a tray filled with <color=#1969ff>liquid blue crystal</color>." },
            {"bluecrystalbags_obtained", "You have <color=#75ff19>successfully obtained {0} bags</color> filled with <color=#1969ff>blue crystal</color>." }
        };
 
        private void Update()
        {
            Frame++;
            if (Frame % 10 != 0) return; 
 
            if (getCurrentTime() - timer >= 1)
            {
                timer = getCurrentTime();
                MethBagFunctions.Update();
                FreezerFunctions.Update();
            }
        }

        public static Int32 getCurrentTime()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public void Wait(float seconds, System.Action action)
        {
            StartCoroutine(_wait(seconds, action));
        }

        private IEnumerator _wait(float time, System.Action callback)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }
    }
}