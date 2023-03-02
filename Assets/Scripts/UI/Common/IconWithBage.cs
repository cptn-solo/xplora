using Assets.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconWithBage : MonoBehaviour
{
    private TextMeshProUGUI bageText;
    private Image iconImage;

    public void SetBadgeText(string text)
    {
        bageText.text = text;
    }

    public void SetIconByCode(BundleIcon code)
    {
        ResolveIcon(iconImage, code);
    }

    public void SetIconColor(Color color)
    {
        iconImage.color = color;
    }

    private void Awake()
    {
        bageText = GetComponentInChildren<TextMeshProUGUI>();
        iconImage = GetComponentInChildren<Image>();
    }

    private void ResolveIcon(Image image, BundleIcon code)
    {
        image.sprite = null;
        image.enabled = false;
        if (code != BundleIcon.NA)
        {
            image.sprite = SpriteForResourceName(code.IconFileName());
            image.enabled = true;
        }
    }

    private Sprite SpriteForResourceName(string iconName)
    {
        var icon = Resources.Load<Sprite>(iconName);
        return icon;
    }
}
