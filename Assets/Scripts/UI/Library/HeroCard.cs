using Assets.Scripts.ECS;
using Assets.Scripts.UI.Common;
using Assets.Scripts.Data;
using Assets.Scripts.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Library
{
    public class HeroCard : BaseEntityView<Hero>
    {

        [SerializeField] private Image heroIconImage;
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private BarsContainer barsContainer;

        private Color normalColor;
        private Image backgroundImage;

        [SerializeField] private Color acceptingColor;
        [SerializeField] private Color selectedColor;

        private Hero hero;
        public Hero Hero
        {
            get => hero;
            set
            {
                hero = value;
                if (hero.HeroType == HeroType.NA)
                {
                    this.gameObject.SetActive(false);
                    return;
                }

                this.gameObject.SetActive(true);

                ResolveIcons();
                heroNameText.text = hero.Name;
                
                barsContainer.SetData(hero.BarsInfoShort);
            }
        }

        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                backgroundImage.color = value ? selectedColor : normalColor;
            }
        }

        private void ResolveIcons()
        {
            ResolveIcon(heroIconImage, Hero);
        }

        private void ResolveIcon(Image image, Hero hero)
        {
            image.sprite = null;
            image.enabled = false;
            if (hero.IconName != null)
            {
                image.sprite = SpriteForResourceName(hero.IconName);
                image.enabled = true;
            }
        }

        private void ResolveIcon(Image image, Asset asset)
        {
            image.sprite = null;
            image.enabled = false;
            if (asset.AssetType != AssetType.NA && asset.IconName != null)
            {
                image.sprite = SpriteForResourceName(asset.IconName);
                image.enabled = true;
            }
        }

        private Sprite SpriteForResourceName(string iconName)
        {
            var icon = Resources.Load<Sprite>(iconName);
            return icon;
        }
        private void Awake()
        {
            backgroundImage = GetComponent<Image>();
            normalColor = backgroundImage.color;
        }

        #region IEntityView

        public override void UpdateData() =>
            Hero = DataLoader(PackedEntity.Value);

        #endregion
    }
}