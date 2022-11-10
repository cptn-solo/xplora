using Assets.Scripts.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Battle Units
    {
        private List<BattleUnit> playerUnits = new();
        private List<BattleUnit> enemyUnits = new();

        public void AddPlayerUnit(BattleUnit unit)
        {
            playerUnits.Add(unit);
            var card = Instantiate(heroPrefab);

            var raidMember = card.GetComponent<RaidMember>();
            raidMember.Unit = unit;

            var actionButton = card.GetComponent<UIActionButton>();
            actionButton.OnActionButtonClick += HeroSelected;

            if (FirstAvailableHeroSlot(unit.Line) is UIItemSlot slot)
                slot.Put(card.GetComponent<RectTransform>());

        }

        private void HeroSelected(Actions action, Transform actionTransform)
        {
            var raidMemeber = actionTransform.GetComponent<RaidMember>();
            Debug.Log($"Hero from line #{raidMemeber.Unit} selected");
            ShowHeroInventory(raidMemeber.Unit);
        }

        private void ShowHeroInventory(BattleUnit unit)
        {
            for (int i = 0; i < heroInventorySlots.Length; i++)
            {
                UIItemSlot slot = heroInventorySlots[i];
                slot.Put(null);
                if (unit.Hero.Inventory.TryGetValue(i, out var asset) &&
                    !asset.Equals(default))
                    slot.Put(ItemForAsset(asset).transform);
            }

            for (int i = 0; i < heroAttackSlots.Length; i++)
            {
                UIItemSlot slot = heroAttackSlots[i];
                slot.Put(null);
                if (unit.Hero.Attack.TryGetValue(i, out var asset) &&
                    !asset.Equals(default))
                    slot.Put(ItemForAsset(asset).transform);

            }

            for (int i = 0; i < heroDefenceSlots.Length; i++)
            {
                UIItemSlot slot = heroDefenceSlots[i];
                slot.Put(null);
                if (unit.Hero.Defence.TryGetValue(i, out var asset) &&
                    !asset.Equals(default))
                    slot.Put(ItemForAsset(asset).transform);

            }
        }
    }
}