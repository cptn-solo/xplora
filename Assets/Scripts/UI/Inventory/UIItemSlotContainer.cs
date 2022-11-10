using UnityEngine;

namespace Assets.Scripts.UI.Inventory
{
    public class UIItemSlotContainer : MonoBehaviour
    {
        private UIItemSlot[] slots;
        private bool initialized;
        private RectTransform rectTransform;

        private void Slot_OnBeginDragItem() =>
            rectTransform.SetAsLastSibling();

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            slots = GetComponentsInChildren<UIItemSlot>();

            foreach (var slot in slots)
            {
                slot.OnBeginDragItem += Slot_OnBeginDragItem;
                slot.OnItemAccepted += Slot_OnItemAccepted;
                slot.OnItemRejected += Slot_OnItemRejected;

                slot.Validator = ValidateSlotItem;
            }

            initialized = true;
        }

        private void Slot_OnItemRejected(Transform arg0, UIItemSlot arg1)
        {
        }

        private void Slot_OnItemAccepted(Transform arg0, UIItemSlot arg1)
        {
        }

        private void OnEnable()
        {
            if (initialized)
                foreach (var slot in slots)
                {
                    slot.OnBeginDragItem += Slot_OnBeginDragItem;
                    slot.OnItemAccepted += Slot_OnItemAccepted;
                    slot.OnItemRejected += Slot_OnItemRejected;
                }

        }

        private void OnDisable()
        {
            foreach (var slot in slots)
            {
                slot.OnBeginDragItem -= Slot_OnBeginDragItem;
                slot.OnItemAccepted -= Slot_OnItemAccepted;
                slot.OnItemRejected -= Slot_OnItemRejected;

            }
        }

        protected virtual bool ValidateSlotItem(Transform itemTransform, UIItemSlot slot)
        {
            return true;
        }
    }
}