using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Inventory
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIItemDraggable : MonoBehaviour//, IDragHandler, IBeginDragHandler, IEndDragHandler, 
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
    }
}