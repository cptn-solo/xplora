using Assets.Scripts.UI.Common;
using Assets.Scripts.UI.Data;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Asset = Assets.Scripts.UI.Data.Asset;

namespace Assets.Scripts.UI.Battle
{
    public class QueueMember : MonoBehaviour
    {
        [SerializeField] private Image priAttackImage;
        [SerializeField] private Image secAttackImage;
        [SerializeField] private Image priDefenceImage;
        [SerializeField] private Image secDefenceImage;

        [SerializeField] private Image heroIconImage;
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private BarsContainer barsContainer;
        [SerializeField] private EffectsContainer effectsContainer;

        private Color normalColor;
        private Image backgroundImage;

        [SerializeField] private Color selectedColor;
        [SerializeField] private Color playerColor;
        [SerializeField] private Color enemyColor;

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

                normalColor = hero.TeamId == 0 ? playerColor : enemyColor;
                backgroundImage.color = normalColor;

                barsContainer.SetData(hero.BarsInfoBattle);
                effectsContainer.SetEffects(hero.Effects);
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
            ResolveIcon(priAttackImage, Hero.Attack[0]);
            ResolveIcon(secAttackImage, Hero.Attack[1]);
            ResolveIcon(priDefenceImage, Hero.Defence[0]);
            ResolveIcon(secDefenceImage, Hero.Defence[1]);

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

    }
}