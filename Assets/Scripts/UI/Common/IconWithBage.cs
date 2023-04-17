using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class IconWithBage : BaseContainableItem<BagedIconInfo>
{
    private TextMeshProUGUI bageText;
    private Image iconImage;

    private Material defaultIconMaterial;

    protected override void OnAwake()
    {
        bageText = GetComponentInChildren<TextMeshProUGUI>();
        iconImage = GetComponentInChildren<Image>();
        defaultIconMaterial = iconImage.material;
    }

    protected override void ApplyInfoValues(BagedIconInfo info)
    {
        ResolveIcon(iconImage, info.Icon);
        bageText.text = info.BadgeText;
        iconImage.color = info.Icon.IconMaterial() == BundleIconMaterial.Font ?
            info.IconColor : Color.white;
    }

    private void ResolveIcon(Image image, BundleIcon code)
    {
        image.sprite = null;
        image.enabled = false;
        if (code != BundleIcon.NA)
        {
            image.sprite = SpriteForResourceName(code.IconFileName());
            image.enabled = true;
            image.material = code.IconMaterial() switch
            {
                BundleIconMaterial.Font => defaultIconMaterial,
                _ => null,
            };
        }
    }

    private Sprite SpriteForResourceName(string iconName)
    {
        var icon = Resources.Load<Sprite>(iconName);
        return icon;
    }
}
