using UnityEngine;
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
        
        private Canvas canvas;

        public int SlotIndex
        {
            get => slotIndex;
            set => slotIndex = value;
        }

        public bool IsEmpty => transform.childCount == 0 || !transform.GetChild(0).gameObject.activeSelf;

        public virtual void OnAwake() { }

        private void Awake()
        {
            backgroundImage = GetComponent<Image>();
            normalColor = backgroundImage.color;
            Color.RGBToHSV(normalColor, out var h, out var s, out var v);
            acceptingColor = Color.HSVToRGB(h, s, v * 1f);
            acceptingColor.a = normalColor.a;
            OnAwake();
        }

        public virtual void Put(Transform itemTransform)
        {
            if (itemTransform == null)
                return;

            itemTransform.SetParent(transform);
            itemTransform.localPosition = Vector3.zero;
            canvas = GetComponentInParent<Canvas>();
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

        public void ToggleVisual(bool toggle)
        { 
            backgroundImage.enabled = toggle;
        }

        public void OnPointerExit(PointerEventData eventData) =>
            SetNormalStyle();

        public void OnDrag(PointerEventData eventData)
        {
            if (dragCargo == null)
                return;

            dragCargo.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!delegateProvider.TransferEnabled(this))
                return;

            if (transform.childCount == 0)
                return;

            dragCargo = delegateProvider.Pool(this).GetComponent<RectTransform>();
            dragCargo.position = transform.position;
            delegateProvider.TransferStart(this, dragCargo);
            dragCargo.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragCargo == null)
            {
                delegateProvider.TransferCleanup(this);
                return;
            }

            dragCargo.GetComponent<CanvasGroup>().blocksRaycasts = true;
            //dragCargo.gameObject.SetActive(false);
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