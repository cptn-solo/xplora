using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Asset = Assets.Scripts.UI.Data.Asset;

namespace Assets.Scripts.UI.Battle
{
    public class RaidMember : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image priAttackImage;
        [SerializeField] private Image secAttackImage;
        [SerializeField] private Image priDefenceImage;
        [SerializeField] private Image secDefenceImage;
        
        [SerializeField] private Image heroIconImage;
        [SerializeField] private TextMeshProUGUI heroNameText;
        
        public HeroDelegateProvider DelegateProvider { 
            get; 
            set; 
        }

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

        public void OnDrop(PointerEventData eventData)
        {
            if (!DelegateProvider.Validator(this) || !DelegateProvider.TransferEnd(this, true))
                DelegateProvider.TransferAbort?.Invoke(this);
        }

        private void Awake()
        {
            backgroundImage = GetComponent<Image>();
            normalColor = backgroundImage.color;
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
                return;

            if (DelegateProvider.Validator(this))
                SetReadyToAcceptItemStyle();
        }

        public void OnPointerExit(PointerEventData eventData) =>
            SetNormalStyle();

        private void SetReadyToAcceptItemStyle() =>
            backgroundImage.color = acceptingColor;

        private void SetNormalStyle() =>
            backgroundImage.color = Selected ? selectedColor : normalColor;

    }
}