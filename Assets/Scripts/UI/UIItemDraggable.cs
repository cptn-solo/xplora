using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class UIItemDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        transform.localScale *= canvas.transform.localScale.x;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var slot = rectTransform.parent.GetComponent<UIItemSlot>();
        slot.HandleDragStart();

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        rectTransform.localPosition = Vector3.zero;
        canvasGroup.blocksRaycasts = true;

    }
}
