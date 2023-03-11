using Assets.Scripts;
using Assets.Scripts.Data;

public partial class IconWithBackgroundAndBage : IEntityViewChild
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
