using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
            UpdateItemAnimated(info, item);

        public void AddItem(T info) =>
            AddItemAnimated(info);

        private void UpdateItemAnimated(T info, I item, Transform sourceTransform = null) 
        {
            ApplyItemInfo(info, item);

            if (sourceTransform == null || item.MovableCard == null)
                return;            

            //Debug.Break();
            StartCoroutine(MoveCoroutine(item, sourceTransform, .5f));
        }

        private IEnumerator MoveCoroutine(I item, Transform source, float sec)
        {

            var destPosition = item.transform.position;
                        
            item.MovableCard.position = source.position;

            var delta = 0f;
            var move =  destPosition - source.position;
            var speed = move / sec;

            while (delta <= sec)
            {
                item.MovableCard.position += speed * Time.deltaTime;

                delta += Time.deltaTime;

                yield return null;
            }

            item.MovableCard.localPosition = Vector3.zero;
        }

        private void AddItemAnimated(T info, Transform sourceTransform = null)
        {
            I item = SpawnItem(sourceTransform == null);

            UpdateItemAnimated(info, item, sourceTransform);

            itemsIndex.Add(info.Id, item);

            var buff = ListPool<I>.Get();

            buff.AddRange(items);
            buff.Add(item);
            items = buff.ToArray();

            ListPool<I>.Add(buff);
        }

        protected I SpawnItem(bool anchorOnSpawn = true)
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