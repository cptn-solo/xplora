using Assets.Scripts.Data;
using System;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public partial class BaseItemsContainer<T, I> : IItemsContainer<T>
        where T : struct, IContainableItemInfo<int> 
        where I : MonoBehaviour, IContainableItem<T>
    {
        public Transform Transform => transform;

        public void SetItemInfo(T info)
        {
            if (itemsIndex.TryGetValue(info.Id, out var item))
                UpdateItem(info, item);
            else
                AddItem(info);
        }

        public void SetItemInfoAnimatedMove(T info, Transform sourceTransform)
        {
            if (itemsIndex.TryGetValue(info.Id, out var item))
                UpdateItemAnimated(info, item, sourceTransform);
            else
                AddItemAnimated(info, sourceTransform);
        }

        public void Reset()
        {
            foreach (var bar in itemsIndex)
                GameObject.Destroy(bar.Value.gameObject);

            itemsIndex.Clear();
            items = new I[0];
        }

        public void SetInfo(T[] value)
        {
            if (value == null)
            {
                Reset();
                return;
            }

            if (items.Length != value.Length)
             {
                foreach (var item in itemsIndex)
                    GameObject.Destroy(item.Value.gameObject);

                itemsIndex.Clear();
                items = new I[value.Length];

                for (int i = 0; i < value.Length; i++)
                {
                    var itemInfo = value[i];
                    I item = SpawnItem();

                    item.SetInfo(itemInfo);

                    items[i] = item;
                    itemsIndex.Add(itemInfo.Id, item);
                }
            }
            else
            {
                for (int i = 0; i < value.Length; i++)
                {
                    var itemInfo = value[i];
                    var item = itemsIndex[itemInfo.Id];
                    item.SetInfo(itemInfo);
                }
            }
        }

        public void RemoveItem(T info)
        {
            if (itemsIndex.TryGetValue(info.Id, out var item))
            {
                GameObject.Destroy(item.gameObject);
                var buff = ListPool<I>.Get();
                buff.AddRange(items);

                buff.Remove(item);
                itemsIndex.Remove(info.Id);

                items = buff.ToArray();

                ListPool<I>.Add(buff);
            }

        }

        public void OnGameObjectDestroy()
        {
            DetachFromEntityView();
        }

    }
}