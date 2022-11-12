using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Inventory
    {
        [SerializeField] private AssetPool assetPool;
        private GameObject ItemForAsset(Asset asset)
        {
            InventoryItem assetCard = assetPool.GetAssetCard(asset.AssetType);

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