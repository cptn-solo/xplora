﻿using Assets.Scripts;
using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public partial class BagedIconsContainer : IItemsContainer<BagedIconInfo>
    {
        public Transform Transform => transform;        

        public void RemoveItem(BagedIconInfo info) =>
            RemoveIcon(info.Icon);

        public void Reset() =>
            ResetIcons();

        public void SetItemInfo(BagedIconInfo info) =>
            SetIconInfo(info);

        public void SetItems(BagedIconInfo[] items)
        {
            foreach (var item in items)
                SetIconInfo(item);
        }

        public void OnGameObjectDestroy() =>
            DetachFromEntityView();
    }
}