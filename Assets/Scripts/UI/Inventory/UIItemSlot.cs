using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Inventory
{

    public class UIItemSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, 
        IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private RectTransform dragCargo;

        private Image backgroundImage;
        private Color normalColor;
        private Color acceptingColor;

        public event UnityAction OnBeginDragItem;

        private SlotDelegateProvider delegateProvider = default;
        public SlotDelegateProvider DelegateProvider { 
            get => delegateProvider; 
            set
            {
                delegateProvider = value;
                Put(delegateProvider.Pool(this));
            }
        }

        [SerializeField] private int slotIndex;
        public int SlotIndex => slotIndex;

        public bool IsEmpty => transform.childCount == 0 || !transform.GetChild(0).gameObject.activeSelf;

        private void Awake()
        {
            backgroundImage = GetComponent<Image>();
            normalColor = backgroundImage.color;
            Color.RGBToHSV(normalColor, out var h, out var s, out var v);
            acceptingColor = Color.HSVToRGB(h, s, v * .7f);
            acceptingColor.a = normalColor.a;
        }

        public virtual void Put(Transform itemTransform)
        {
            itemTransform.SetParent(transform);
            itemTransform.localPosition = Vector3.zero;
        }

        private void SetReadyToAcceptItemStyle() =>
            backgroundImage.color = acceptingColor;

        private void SetNormalStyle() =>
            backgroundImage.color = normalColor;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            if (delegateProvider.Validator(this))
                SetReadyToAcceptItemStyle();

        }

        public void OnPointerExit(PointerEventData eventData) =>
            SetNormalStyle();

        public void OnDrag(PointerEventData eventData) =>
            dragCargo.position = eventData.position;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (transform.childCount == 0)
                return;

            dragCargo = delegateProvider.Pool(this).GetComponent<RectTransform>();
            delegateProvider.TransferStart(this, dragCargo);
            dragCargo.GetComponent<CanvasGroup>().blocksRaycasts = false;
            dragCargo.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragCargo.GetComponent<CanvasGroup>().blocksRaycasts = true;
            dragCargo.gameObject.SetActive(false);
            dragCargo = null;

            delegateProvider.TransferCleanup(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!delegateProvider.Validator(this) || !delegateProvider.TransferEnd(this))
                delegateProvider.TransferAbort?.Invoke(this);
        }
    }
}