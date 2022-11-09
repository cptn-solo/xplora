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

            if (FirstAvailableSlot(unit.Line) is UIItemSlot slot)
                slot.Put(card.GetComponent<RectTransform>());

        }

        private void HeroSelected(Actions action, Transform actionTransform)
        {
            var raidMemeber = actionTransform.GetComponent<RaidMember>();
            Debug.Log($"Hero from line #{raidMemeber.Unit} selected");
        }
    }
}