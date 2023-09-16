using Assets.Scripts.Data;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts.ECS;

namespace Assets.Scripts.World
{
    public partial class TeamMember : BaseEntityView<TeamMemberInfo>
    {
        [SerializeField] private Image heroIconImage;
        [SerializeField] private Image heroIdleImage;

        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI scoreText;

        private Color normalColor;
        private Image backgroundImage;
        
        private TeamMemberInfo? hero;
        public TeamMemberInfo? Hero
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
                backgroundImage.color = normalColor;
            }
        }

        private void ResolveIcons()
        {
            ResolveIcons(heroIconImage, heroIdleImage, Hero.Value);
        }

        private void ResolveIcons(Image image, Image idle, TeamMemberInfo hero)
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

        protected override void OnBeforeAwake()
        {
            backgroundImage = GetComponent<Image>();
            normalColor = backgroundImage.color;
        }

    }
}