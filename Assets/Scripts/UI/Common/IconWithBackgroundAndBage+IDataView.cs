using Assets.Scripts;
using Assets.Scripts.Data;
using UnityEngine;

public partial class IconWithBackgroundAndBage : IDataView<BagedIconInfo>
{
    public Transform Transform => transform;

    public void OnGameObjectDestroy() =>
        DetachFromEntityView();

    public void Reset()
    {
        SetBadgeText("");
        bgImage.enabled = false;
        iconImage.enabled = false;
    }

    public void SetInfo(BagedIconInfo info)
    {
        SetBadgeText(info.BadgeText);
        SetIconByCode(info.Icon);
        SetIconColor(info.IconColor);
        SetBackgroundColor(info.BackgroundColor);
    }
}
