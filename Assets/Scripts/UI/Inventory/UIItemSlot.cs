using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Inventory
{
    public delegate bool AcceptableItemChecker(Transform item);
    public delegate void AssetTransferStartDelegate<T>(Transform item, T slot);
    public delegate bool AssetTransferEndDelegate(Transform item, UIItemSlot slot, bool accepted);
    public delegate bool AssetTransferAbortDelegate();

    public class UIItemSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler
    {        
        private RectTransform rectTransform;
        private Image backgroundImage;
        private Color normalColor;
        private Color acceptingColor;

        public event UnityAction OnBeginDragItem;

        public AcceptableItemChecker Validator;
        
        public AssetTransferStartDelegate<UIItemSlot> TransactionStart;
        public AssetTransferEndDelegate TransactionEnd;
        public AssetTransferAbortDelegate TransactionAbort;

        [SerializeField] private int slotIndex;
        public int SlotIndex => slotIndex;

        public bool IsEmpty => transform.childCount == 0;

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
            
            TransactionStart(Take(), this);
        }

        public void Put(Transform itemTransform)
        {
            if (itemTransform == null)
            {
                if (this.transform.childCount == 0)
                    return;

                var cargo = Take();
                cargo.gameObject.SetActive(false);

                return;
            }

            var prevParent = itemTransform.parent;
            if (prevParent != null &&
                prevParent.GetComponent<UIItemSlot>() is UIItemSlot prevSlot)
                prevSlot.Take();

            itemTransform.SetParent(transform);
            itemTransform.localPosition = Vector3.zero;
        }

        public Transform Take()
        {
            if (transform.childCount > 0 && 
                transform.GetChild(0).TryGetComponent<InventoryItem>(out var cargo))
            {
                cargo.transform.SetParent(null);
                return cargo.transform;
            }
            return null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var cargo = eventData.pointerDrag.transform;
            if (Validator(cargo) && TransactionEnd(cargo, this, true))
            {
                Put(cargo);
            }
            else
            {
                TransactionAbort?.Invoke();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            var cargo = eventData.pointerDrag.transform;
            if (Validator(cargo))
                SetReadyToAcceptItemStyle();

        }

        public void OnPointerExit(PointerEventData eventData) =>
            SetNormalStyle();

        private void SetReadyToAcceptItemStyle() =>
            backgroundImage.color = acceptingColor;

        private void SetNormalStyle() =>
            backgroundImage.color = normalColor;

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log($"slot drag {eventData.delta}");
        }
    }
}