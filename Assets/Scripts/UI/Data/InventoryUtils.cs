using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Data
{
    public static class InventoryUtils
    {
        public delegate bool ItemComparer<T>(T val);
        public static Tuple<int, BattleLine, int> FirstFreeSlotIndex<T>(
            this Dictionary<Tuple<int, BattleLine, int>, T> heroes, 
            ItemComparer<T> comparer)
        {
            var sorted = heroes.OrderBy(x => x.Key).ToArray();
            for (int i = 0; i < sorted.Length; i++)
            {
                var slot = sorted[i];
                if (comparer(slot.Value))
                    return sorted[i].Key;
            }

            return new(-1, BattleLine.NA, -1);
        }        

        public static int FirstFreeSlotIndex<T>(
            this Dictionary<int, T> inventory, 
            ItemComparer<T> comparer)
        {
            var sorted = inventory.OrderBy(x => x.Key).ToArray();
            for (int i = 0; i < sorted.Length; i++)
            {
                var slot = sorted[i];
                if (comparer(slot.Value))
                    return i;
            }

            return -1;
        }


        public static int PutAsset(this Dictionary<int, Asset> inventory, Asset asset, int index)
        {
            var idx = index >= 0 ? index : 
                inventory.FirstFreeSlotIndex(x => x.AssetType == AssetType.NA);
            
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

        public static Asset TakeAsset(this Dictionary<int, Asset> from, int idx)
        {
            if (idx >= 0 &&
                from[idx] is Asset current &&
                current.AssetType != AssetType.NA)
            {
                from[idx] = default;
                return current;
            }
            return default;
        }
        public static Asset Copy(this Asset asset)
        {
            Asset cp = default;
            return cp.CopyFrom(asset);
        }

    }


}