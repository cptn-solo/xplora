using UnityEngine;
using UnityEngine.EventSystems;

public class UIItemSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var cargo = eventData.pointerDrag.transform;
        cargo.SetParent(transform);
        cargo.localPosition = Vector3.zero;
    }

    
}
