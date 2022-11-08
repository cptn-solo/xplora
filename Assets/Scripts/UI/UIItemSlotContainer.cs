using Assets.Scripts.UI;
using UnityEngine;

public class UIItemSlotContainer : MonoBehaviour
{
    private UIItemSlot[] slots;
    private bool initialized;
    private RectTransform rectTransform;

    private void Slot_OnBeginDragItem()
    {
        rectTransform.SetAsLastSibling();
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        slots = GetComponentsInChildren<UIItemSlot>();
        
        foreach (var slot in slots)
            slot.OnBeginDragItem += Slot_OnBeginDragItem;

        initialized = true;
    }

    private void OnEnable()
    {
        if (initialized)
            foreach (var slot in slots)
                slot.OnBeginDragItem += Slot_OnBeginDragItem;
    }

    private void OnDisable()
    {
        foreach (var slot in slots)
            slot.OnBeginDragItem -= Slot_OnBeginDragItem;
    }
}
