using Assets.Scripts.ECS;
using Assets.Scripts.UI.Common;
using Assets.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Assets.Scripts.Services;
using System.Collections;

namespace Assets.Scripts.UI.Library
{
    public partial class HeroCard : BaseEntityView<Hero>        
    {
        private HeroLibraryService libraryService;

        [Inject]
        public void Construct(HeroLibraryService libraryService)
        {
            this.libraryService = libraryService;
        }

        [SerializeField] private Image heroIconImage;
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private BarsContainer barsContainer;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI sliderValue;
        [SerializeField] private Button button;

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
                
                barsContainer.SetInfo(hero.BarsInfoShort);
            }
        }

        private bool selected;
        private float lastCapturedSliderValue;
        private Coroutine sliderValueCoroutine;

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

        private Sprite SpriteForResourceName(string iconName)
        {
            var icon = Resources.Load<Sprite>(iconName);
            return icon;
        }

        protected override void OnBeforeAwake()
        {
            backgroundImage = GetComponent<Image>();
            normalColor = backgroundImage.color;

            button.onClick.AddListener(OnCardClicked);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        protected override void OnBeforeDestroy()
        {
            button.onClick.RemoveListener(OnCardClicked);
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            lastCapturedSliderValue = value;
            sliderValueCoroutine ??= StartCoroutine(DelaySliderValueChange());
        }

        private IEnumerator DelaySliderValueChange()
        {
            while (true)

            {
                yield return new WaitForSeconds(.5f);

                if (slider.value == lastCapturedSliderValue) 
                    break;
            }

            if (PackedEntity.HasValue)
            {
                sliderValue.text = ((int)lastCapturedSliderValue).ToString();
                libraryService.SetRelationScore(PackedEntity.Value, lastCapturedSliderValue);
                sliderValueCoroutine = null;
            }

        }

        private void OnCardClicked()
        {
            if (PackedEntity.HasValue)
                libraryService.SetSelectedHero(PackedEntity.Value);
        }

        public void ToggleSliderVisibility(bool toggle) { 
            slider.gameObject.SetActive(toggle);
        }

        public void SetSliderValue(int value)
        {
            sliderValue.text = value.ToString();
            slider.SetValueWithoutNotify(value);
        }
    }
}