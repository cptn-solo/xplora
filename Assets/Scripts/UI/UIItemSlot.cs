using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class UIItemSlot : MonoBehaviour, IDropHandler
    {
        private RectTransform rectTransform;

        public event UnityAction OnBeginDragItem;

        private Transform itemTransform = null;
        public Transform ItemTransform => itemTransform;

        [SerializeField] private int slotIndex;
        public int SlotIndex => slotIndex;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        public void HandleDragStart()
        {
            rectTransform.SetAsLastSibling();
            OnBeginDragItem?.Invoke();
        }

        public void Put(Transform itemTransform)
        {
            this.itemTransform = itemTransform;

            itemTransform.SetParent(transform);
            itemTransform.localPosition = Vector3.zero;
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
            Put(cargo);
        }


    }
}