using Assets.Scripts.UI;
using Assets.Scripts.UI.Battle;
using UnityEngine;

public class BattleLineSlotsContainer : UIItemSlotContainer
{
    protected override bool ValidateSlotItem(Transform itemTransform, UIItemSlot slot)
    {
        var hero = itemTransform.GetComponent<RaidMember>();
        return hero != null;
    }
}
