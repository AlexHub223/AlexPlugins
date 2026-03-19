using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlueCrystalCooking.functions
{
    public static class FreezerFunctions
    {
        public static void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (asset.id == BlueCrystalCookingPlugin.Instance.Configuration.Instance.LiquidTrayId)
            {
                Vector3 targetPos = point;
                ulong ownerId = owner;
                ulong groupId = group;
                float ax = angle_x;
                float ay = angle_y;
                float az = angle_z;

                BlueCrystalCookingPlugin.Instance.Wait(0.5f, () =>
                {
                    BarricadeDrop trayDrop = BarrelFunctions.FindNearbyBarricade(targetPos, BlueCrystalCookingPlugin.Instance.Configuration.Instance.LiquidTrayId, 0.5f);
                    if (trayDrop != null)
                    {
                        BarricadeDrop freezerDrop = BarrelFunctions.FindNearbyBarricade(targetPos, BlueCrystalCookingPlugin.Instance.Configuration.Instance.FreezerId, 3.0f);
                        
                        if (freezerDrop != null)
                        {
                            if (!BlueCrystalCookingPlugin.Instance.freezingTrays.Any(t => t.instanceID == trayDrop.instanceID))
                            {
                                BlueCrystalCookingPlugin.Instance.freezingTrays.Add(new FreezingTrayObject(trayDrop.instanceID, targetPos, ownerId, groupId, ax, ay, az, 0));
                            }
                        }
                    }
                });
            }
        }

        public static void Update()
        {
            if (BlueCrystalCookingPlugin.Instance.freezingTrays.Count == 0) return;

            foreach (var tray in BlueCrystalCookingPlugin.Instance.freezingTrays.ToList())
            {
                bool hasPower = true;
                if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.FreezerNeedsPower)
                {
                    hasPower = false;
                    List<InteractableGenerator> generators = PowerTool.checkGenerators(tray.pos, PowerTool.MAX_POWER_RANGE, ushort.MaxValue);
                    if (generators != null)
                    {
                        foreach (var generator in generators)
                        {
                            if (generator.fuel > 0 && generator.isPowered && generator.wirerange >= (tray.pos - generator.transform.position).magnitude)
                            {
                                hasPower = true;
                                break;
                            }
                        }
                    }
                }

                if (hasPower)
                {
                    tray.freezingSeconds += 1;
                    if (tray.freezingSeconds >= BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayFreezingTimeSecs)
                    {
                        BarricadeDrop foundDrop = null;
                        foreach (var region in BarricadeManager.regions)
                        {
                            foundDrop = region.drops.FirstOrDefault(d => d.instanceID == tray.instanceID);
                            if (foundDrop != null) break;
                        }

                        if (foundDrop == null)
                        {
                            foreach (var region in BarricadeManager.vehicleRegions)
                            {
                                foundDrop = region.drops.FirstOrDefault(d => d.instanceID == tray.instanceID);
                                if (foundDrop != null) break;
                            }
                        }

                        if (foundDrop != null)
                        {
                            if (BarricadeManager.tryGetInfo(foundDrop.model, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region))
                            {
                                BarricadeManager.destroyBarricade(region, x, y, plant, index);
                                
                                ItemBarricadeAsset frozenAsset = (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, BlueCrystalCookingPlugin.Instance.Configuration.Instance.FrozenTrayId);
                                if (frozenAsset != null)
                                {
                                    BarricadeManager.dropBarricade(new Barricade(frozenAsset), null, tray.pos, tray.angle_x, tray.angle_y, tray.angle_z, tray.owner, tray.group);
                                    
                                    if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.EnableBlueCrystalFreezeEffect)
                                    {
                                        EffectManager.sendEffect(BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalFreezeEffectId, 10, tray.pos);
                                    }
                                }
                            }
                        }
                        BlueCrystalCookingPlugin.Instance.freezingTrays.Remove(tray);
                    }
                }
            }
        }
    }
}
