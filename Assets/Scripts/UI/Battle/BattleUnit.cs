using Assets.Scripts.ECS;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Asset = Assets.Scripts.Data.Asset;
using Zenject;
using Assets.Scripts.Services;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleUnit : BaseEntityView<Hero>,
        IPointerEnterHandler, IPointerExitHandler
    {        
        [SerializeField] private HeroAnimation heroAnimation;
        [Inject] private readonly BattleManagementService battleService;

        public HeroAnimation HeroAnimation => heroAnimation;

        private Hero hero;
        public Hero Hero => hero;

        public void SetHero(Hero? hero)
        {
            this.hero = hero == null ? default : hero.Value;
            if (hero == null)
            {
                this.gameObject.SetActive(false);

                if (heroAnimation != null)
                    heroAnimation.SetHero(hero, IsPlayerTeam);

                return;
            }

            this.gameObject.SetActive(true);

            if (heroAnimation != null)
            {
                heroAnimation.SetHero(hero, IsPlayerTeam);
                heroAnimation.Initialize();
            }
        }

        public bool IsPlayerTeam { get; internal set; }

        private void OnDisable()
        {
            if (heroAnimation != null)
                heroAnimation.HideOverlay();
        }

        public void OnPointerEnter(PointerEventData eventData) =>
            battleService.RequestDetailsHover(PackedEntity);

        public void OnPointerExit(PointerEventData eventData) =>
            battleService.DismissDetailsHover(PackedEntity);


        private void OnDestroy()
        {
            OnGameObjectDestroy();
        }

    }
}