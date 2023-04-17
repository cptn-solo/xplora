using Assets.Scripts.Data;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace Assets.Scripts.UI.Common
{
    public partial class BaseItemsContainer<T, I> : MonoBehaviour
        where T : struct, IContainableItemInfo<int> 
        where I : MonoBehaviour, IContainableItem<T>
    {
        [SerializeField] protected GameObject prefab;

        protected readonly Dictionary<int, I> itemsIndex = new();
        protected I[] items = new I[0];

        protected RectTransform rectTransform;

        protected void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            OnAwake();
        }

        protected virtual void OnAwake() { }
        protected virtual void ApplyItemInfo(T info, I item) =>
            item.SetInfo(info);

        public void UpdateItem(T info, I item) =>
            ApplyItemInfo(info, item);

        public void AddItem(T info)
        {
            I item = SpawnItem();

            UpdateItem(info, item);

            itemsIndex.Add(info.Id, item);

            var buff = ListPool<I>.Get();
            
            buff.AddRange(items);
            buff.Add(item);
            items = buff.ToArray();
         
            ListPool<I>.Add(buff);                        
        }

        protected I SpawnItem()
        {
            var item = Instantiate(prefab).GetComponent<I>();
            var rectTransform = item.GetComponent<RectTransform>();
            var canvas = GetComponentInParent<Canvas>();            
            rectTransform.localScale = canvas.transform.localScale * transform.parent.localScale.x;
            rectTransform.SetParent(this.rectTransform);            
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchoredPosition3D = Vector3.zero;
            return item;
        }

        protected void OnDestroy() =>
            OnGameObjectDestroy();
    }
}