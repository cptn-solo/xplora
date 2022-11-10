using Assets.Scripts.UI.Inventory;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class BattleLineSlotsContainer : UIItemSlotContainer
    {
        protected override bool ValidateSlotItem(Transform itemTransform, UIItemSlot slot)
        {
            var hero = itemTransform.GetComponent<RaidMember>();
            return hero != null;
        }
    }
}