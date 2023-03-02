using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Common
{
    public partial class BarsContainer : IEntityViewChild
    {
        public void AttachToEntityView()
        {
            if (GetComponentInParent<IEntityView>(true) is IEntityView entityView)
                entityView.AttachChild<BarInfo>(this);
        }

        public void DetachFromEntityView()
        {
            if (GetComponentInParent<IEntityView>(true) is IEntityView entityView)
                entityView.DetachChild<BarInfo>(this);
        }

    }
}