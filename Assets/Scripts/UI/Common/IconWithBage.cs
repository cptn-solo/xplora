using Assets.Scripts;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class IconWithBage : BaseContainableItem<BagedIconInfo>
{
    private TextMeshProUGUI bageText;
    private Image iconImage;

    protected override Image IconImage => iconImage;

    protected override void OnAwake()
    {
        bageText = GetComponentInChildren<TextMeshProUGUI>();
        iconImage = GetComponentInChildren<Image>();
    }

    protected override void ApplyInfoValues(BagedIconInfo info)
    {
        ResolveIcon(iconImage, info.Icon);
        bageText.text = info.BadgeText;
        iconImage.color = info.Icon.IconMaterial() == BundleIconMaterial.Font ?
            info.IconColor : Color.white;
    }

    
}
