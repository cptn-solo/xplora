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

            if (FirstAvailableSlot(unit.Line) is UIItemSlot slot)
                slot.Put(card.GetComponent<RectTransform>());

        }


    }
}