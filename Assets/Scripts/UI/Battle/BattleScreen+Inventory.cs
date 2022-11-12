using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Inventory
    {
        private readonly Dictionary<AssetType, List<InventoryItem>> assetPool = new();
        private GameObject ItemForAsset(Asset asset)
        {
            List<InventoryItem> cachedItems = null;
            InventoryItem assetCard = null;

            if (!assetPool.TryGetValue(asset.AssetType, out cachedItems))
            {
                cachedItems = new();
                assetCard = Instantiate(itemPrefab).GetComponent<InventoryItem>();
                cachedItems.Add(assetCard);
            }

            if (cachedItems.Where(x => !x.enabled).FirstOrDefault() is InventoryItem instance)
            {
                if (instance == null)
                {
                    assetCard = Instantiate(itemPrefab).GetComponent<InventoryItem>();
                    cachedItems.Add(assetCard);
                }
                else
                {
                    assetCard = instance;
                    assetCard.transform.SetParent(null);
                    assetCard.gameObject.SetActive(true);
                }                    
            }

            assetCard.Asset = asset;

            return assetCard.gameObject;
        }
        private void ShowTeamInventory(Team team)
        {
            foreach (var asset in team.Inventory)
                teamInventorySlots[asset.Key].Put(
                    asset.Value.AssetType == AssetType.NA ? null :
                    ItemForAsset(asset.Value).transform);
        }
        private void ShowHeroInventory(Hero hero)
        {
            heroInventoryTitle.text = hero.Name;
            for (int i = 0; i < heroInventorySlots.Length; i++)
            {
                UIItemSlot slot = heroInventorySlots[i];
                slot.Put(null);
                if (hero.Inventory.TryGetValue(i, out var asset) &&
                    asset.AssetType != AssetType.NA)
                    slot.Put(ItemForAsset(asset).transform);
            }

            for (int i = 0; i < heroAttackSlots.Length; i++)
            {
                UIItemSlot slot = heroAttackSlots[i];
                slot.Put(null);
                if (hero.Attack.TryGetValue(i, out var asset) &&
                    asset.AssetType != AssetType.NA)
                    slot.Put(ItemForAsset(asset).transform);

            }

            for (int i = 0; i < heroDefenceSlots.Length; i++)
            {
                UIItemSlot slot = heroDefenceSlots[i];
                slot.Put(null);
                if (hero.Defence.TryGetValue(i, out var asset) &&
                    asset.AssetType != AssetType.NA)
                    slot.Put(ItemForAsset(asset).transform);

            }
        }

    }
}