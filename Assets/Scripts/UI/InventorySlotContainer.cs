using Assets.Scripts.UI;
using UnityEngine;
using Assets.Scripts;

public class InventorySlotContainer : UIItemSlotContainer
{
    protected override bool ValidateSlotItem(Transform itemTransform, UIItemSlot slot)
    {
        var inventoryItem = itemTransform.GetComponent<InventoryItem>();
        return inventoryItem != null;
    }

}
