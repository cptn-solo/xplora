using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public partial class BaseItemsContainer<T, I> : IEntityViewChild
        where T : struct, IContainableItemInfo<int> 
        where I : MonoBehaviour, IContainableItem<T>
    {
        public void AttachToEntityView()
        {
            if (GetComponentInParent<IEntityView>(true) is IEntityView entityView)
                entityView.AttachChild<T>(this);
        }

        public void DetachFromEntityView()
        {
            if (GetComponentInParent<IEntityView>(true) is IEntityView entityView)
                entityView.DetachChild<T>(this);
        }

    }
}