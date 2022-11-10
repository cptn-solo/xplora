using UnityEngine;

namespace Assets.Scripts.UI.Inventory
{
    public class InventorySlotContainer : UIItemSlotContainer
    {
        protected override bool ValidateSlotItem(Transform itemTransform, UIItemSlot slot)
        {
            var inventoryItem = itemTransform.GetComponent<InventoryItem>();
            return inventoryItem != null;
        }

    }
}