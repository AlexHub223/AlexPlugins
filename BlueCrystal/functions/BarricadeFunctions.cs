using SDG.Unturned;
using UnityEngine;

namespace BlueCrystalCooking.functions
{
    public static class BarricadeFunctions
    {
        public static void BarricadeDamaged(Transform barricadeTransform, ushort pendingTotalDamage)
        {
            if (barricadeTransform == null) return;

            if (BarricadeManager.tryGetInfo(barricadeTransform, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region, out BarricadeDrop drop))
            {
                BarricadeData data = region.barricades[index];
                if (data.barricade.health <= pendingTotalDamage)
                {
                    if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.BarrelObjectId == drop.asset.id)
                    {
                        BlueCrystalCookingPlugin.Instance.placedBarrelsIngredients.Remove(drop.instanceID);
                    }
                    else if (BlueCrystalCookingPlugin.Instance.Configuration.Instance.BlueCrystalTrayId == drop.asset.id || 
                             BlueCrystalCookingPlugin.Instance.Configuration.Instance.LiquidTrayId == drop.asset.id)
                    {
                        BlueCrystalCookingPlugin.Instance.freezingTrays.RemoveAll(t => t.instanceID == drop.instanceID);
                    }
                }
            }
        }
    }
}
