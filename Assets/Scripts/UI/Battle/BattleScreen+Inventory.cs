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
            InventoryItem assetCard = assetPool.GetAssetCard(asset);

            assetCard.Asset = asset;

            return assetCard.gameObject;
        }
        private void ShowTeamInventory(Team team)
        {
            foreach (var asset in team.Inventory)
                teamInventorySlots[asset.Key].Asset = asset.Value;
        }
        private void ShowHeroInventory(Hero hero)
        {
            heroInventoryTitle.text = hero.Name;
            for (int i = 0; i < heroInventorySlots.Length; i++)
                heroInventorySlots[i].Asset = hero.Inventory[i];
            
            for (int i = 0; i < heroAttackSlots.Length; i++)
                heroAttackSlots[i].Asset = hero.Attack[i];

            for (int i = 0; i < heroDefenceSlots.Length; i++)
                heroDefenceSlots[i].Asset = hero.Defence[i];
        }
    }
}