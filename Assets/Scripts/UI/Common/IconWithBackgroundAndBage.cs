using Assets.Scripts;
using Assets.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class IconWithBackgroundAndBage : MonoBehaviour
{
    private TextMeshProUGUI bageText;

    [SerializeField] private Image iconImage;
    [SerializeField] private Image bgImage;

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

    public void SetBackgroundColor(Color? color)
    {
        bgImage.enabled = color != null;
        if (color != null)
            bgImage.color = color.Value;
    }

    private void Awake()
    {
        bageText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        OnGameObjectDestroy();
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
