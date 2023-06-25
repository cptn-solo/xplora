using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using UnityEngine;
using Zenject;
using Image = UnityEngine.UI.Image;

public class BundleIconHost : BaseContainableItem<BundleIconInfo>
{
    public class Factory : PlaceholderFactory<BundleIconHost> { }
    
    private Image iconImage;
    protected override Image IconImage => iconImage;
    protected override void OnAwake()
    {
        iconImage = GetComponent<Image>();
    }

    protected override void ApplyInfoValues(BundleIconInfo info)
    {
        ResolveIcon(iconImage, info.Icon);
        iconImage.color = info.Icon.IconMaterial() == BundleIconMaterial.Font ?
            info.IconColor : Color.white;
    }
}
