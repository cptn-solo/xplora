using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Common
{
    public partial class BagedIconsContainer : IEntityViewChild
    {
        public void AttachToEntityView()
        {
            if (GetComponentInParent<IEntityView>(true) is IEntityView entityView)
                entityView.AttachChild<BagedIconInfo>(this);
        }

        public void DetachFromEntityView()
        {
            if (GetComponentInParent<IEntityView>(true) is IEntityView entityView)
                entityView.DetachChild<BagedIconInfo>(this);
        }

    }
}