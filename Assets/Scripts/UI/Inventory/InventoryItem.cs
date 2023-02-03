using Assets.Scripts.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Inventory
{
    public class InventoryItem : MonoBehaviour
    {
        private Asset asset;
        private Image image;

        public Asset Asset
        {
            get => asset;
            set
            {
                asset = value;
                if (asset.AssetType == AssetType.NA)
                {
                    gameObject.SetActive(false);
                    return;
                }
                
                gameObject.SetActive(true);

                ResolveIcons();
            }
        }

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void ResolveIcons()
        {
            ResolveIcon(image, asset);
        }

        private void ResolveIcon(Image image, Asset asset)
        {
            image.sprite = null;
            image.enabled = false;
            if (asset.AssetType != AssetType.NA && asset.IconName != null)
            {
                image.sprite = SpriteForResourceName($"Icons/Assets/{asset.IconName}");
                image.enabled = true;
            }
        }

        private Sprite SpriteForResourceName(string iconName)
        {
            var icon = Resources.Load<Sprite>(iconName);
            return icon;
        }
    }
}