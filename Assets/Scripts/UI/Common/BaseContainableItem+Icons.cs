using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Common
{
    public partial class BaseContainableItem<T>
        where T : struct
    {
        
        protected Material defaultIconMaterial;
        protected virtual Image IconImage { get { return null; } }


        /// <summary>
        /// wrapped by the item container so container is positioned in the collection, while the card can be moved and then
        /// just parked back
        /// </summary>
        public virtual Transform MovableCard { get { return null; } }

        protected void InitIconUtils()
        {
            defaultIconMaterial = IconImage != null ? IconImage.material : null;
        }

        protected void ResolveIcon(Image image, BundleIcon code)
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

        protected Sprite SpriteForResourceName(string iconName)
        {
            var icon = Resources.Load<Sprite>(iconName);
            return icon;
        }
    }
}