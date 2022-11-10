using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Data
{
    public static class InventoryUtils
    {
        public static int FirstFreeSlotIndex(this Dictionary<int, Asset> inventory)
        {
            var sorted = inventory.OrderBy(x => x.Key).ToArray();
            for (int i = 0; i < sorted.Length; i++)
            {
                var slot = sorted[i];
                if (slot.Value.AssetType == AssetType.NA)
                    return i;
            }

            return -1;
        }

        public static int PutAsset(this Dictionary<int, Asset> inventory, Asset asset, int index)
        {
            var idx = index >= 0 ? index : inventory.FirstFreeSlotIndex();
            
            if (idx >= 0 &&
                inventory[idx] is Asset current && 
                (current.AssetType == AssetType.NA || current.AssetType == asset.AssetType))
            {
                var extra = current.Count + asset.Count - asset.MaxCount;
                current.CopyFrom(asset);
                current.Count = Mathf.Max(asset.MaxCount, current.Count + asset.Count);
                current.Owner = asset.Owner;
                
                inventory[idx] = current;
                
                return Mathf.Max(extra, 0);
            }

            return asset.Count;
        }
    }


}