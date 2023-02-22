using Assets.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Battle
{
    public class QueueMember : MonoBehaviour
    {       
        [SerializeField] private Image heroIconImage;
        [SerializeField] private Image heroIdleImage;

        [SerializeField] private TextMeshProUGUI heroNameText;

        private Color normalColor;
        private Image backgroundImage;

        [SerializeField] private Color playerColor;
        [SerializeField] private Color enemyColor;

        private RoundSlotInfo? hero;
        public RoundSlotInfo? Hero
        {
            get => hero;
            set
            {
                hero = value;
                if (hero == null)
                {
                    gameObject.SetActive(false);
                    return;
                }

                gameObject.SetActive(true);

                ResolveIcons();
                heroNameText.text = hero.Value.HeroName;

                normalColor = hero.Value.TeamId == 0 ? playerColor : enemyColor;
                backgroundImage.color = normalColor;
            }
        }

        private void ResolveIcons()
        {
            ResolveIcons(heroIconImage, heroIdleImage, Hero.Value);
        }

        private void ResolveIcons(Image image, Image idle, RoundSlotInfo hero)
        {
            image.sprite = null;
            idle.sprite = null;
            image.enabled = false;
            idle.enabled = false;
            if (hero.IconName != null)
            {
                image.sprite = SpriteForResourceName(hero.IconName);
                image.enabled = true;
            }
            if (hero.IdleSpriteName != null)
            {
                idle.sprite = SpriteForResourceName(hero.IdleSpriteName);
                idle.enabled = true;
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