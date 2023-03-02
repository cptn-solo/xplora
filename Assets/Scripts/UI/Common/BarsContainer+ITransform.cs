using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public partial class BarsContainer : IItemsContainer<BarInfo>
    {
        public Transform Transform => transform;

        public void SetItemInfo(BarInfo info)
        {
            UpdateBar(info);
        }

        public void Reset()
        {
            foreach (var bar in barsIndex)
                GameObject.Destroy(bar.Value.gameObject);

            barsIndex.Clear();
            bars = new BarWithTitle[0];
        }

        public void SetItems(BarInfo[] items)
        {
            SetData(items);
        }

        public void RemoveItem(BarInfo info)
        {
            throw new System.NotImplementedException();
        }

        public void OnGameObjectDestroy()
        {
            DetachFromEntityView();
        }
    }
}