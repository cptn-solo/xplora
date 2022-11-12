using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Inventory
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIItemDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas canvas;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }
        void Start()
        {
            canvas = GetComponentInParent<Canvas>();

            if (transform.parent != null && canvas != null)
                transform.localScale *= canvas.transform.localScale.x;
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            var slot = GetComponentInParent<UIItemSlot>();
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
}