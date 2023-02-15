using Assets.Scripts.Data;
using Assets.Scripts.UI.Inventory;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Inventory
    {
        [SerializeField] private AssetPool assetPool;
        private void ShowTeamInventory(Team team)
        {
            var slots = (team.Id == playerTeamId) ?
                playerTeamInventorySlots : enemyTeamInventorySlots;
            foreach (var asset in team.Inventory)
                slots[asset.Key].Asset = asset.Value;
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