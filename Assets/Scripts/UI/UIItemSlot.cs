using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public delegate bool AcceptableItemChecker(Transform item, UIItemSlot slot);

    public class UIItemSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform rectTransform;
        private Image backgroundImage;
        private Color normalColor;
        private Color acceptingColor;

        public event UnityAction OnBeginDragItem;
        public event UnityAction<Transform, UIItemSlot> OnItemAccepted;
        public event UnityAction<Transform, UIItemSlot> OnItemRejected;
        
        public AcceptableItemChecker Validator;

        private Transform itemTransform = null;
        public Transform ItemTransform => itemTransform;

        [SerializeField] private int slotIndex;

        public int SlotIndex => slotIndex;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            backgroundImage = GetComponent<Image>();
            normalColor = backgroundImage.color;
            Color.RGBToHSV(normalColor, out var h, out var s, out var v);
            acceptingColor = Color.HSVToRGB(h, s, v * .7f);
            acceptingColor.a = normalColor.a;
        }
        public void HandleDragStart()
        {
            rectTransform.SetAsLastSibling();
            OnBeginDragItem?.Invoke();
        }

        public void Put(Transform itemTransform)
        {
            var prevParent = itemTransform.parent;
            if (prevParent != null && 
                prevParent.GetComponent<UIItemSlot>() is UIItemSlot prevSlot)
                prevSlot.Take();
            
            itemTransform.SetParent(transform);
            itemTransform.localPosition = Vector3.zero;

            this.itemTransform = itemTransform;
        }

        public Transform Take()
        {
            var cargo = itemTransform;
            itemTransform = null;
            return cargo;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var cargo = eventData.pointerDrag.transform;
            if(CheckIfItemAccpetableForSlot(cargo))
            {
                Put(cargo);
                OnItemAccepted?.Invoke(itemTransform, this);
            }
            else
            {
                OnItemRejected?.Invoke(itemTransform, this);
            }
        }

        private bool CheckIfItemAccpetableForSlot(Transform cargo) => 
            Validator(cargo, this);

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            var cargo = eventData.pointerDrag.transform;
            if (CheckIfItemAccpetableForSlot(cargo))
                SetReadyToAcceptItemStyle();

        }

        public void OnPointerExit(PointerEventData eventData) =>
            SetNormalStyle();

        private void SetReadyToAcceptItemStyle() =>
            backgroundImage.color = acceptingColor;

        private void SetNormalStyle() =>
            backgroundImage.color = normalColor;
    }
}