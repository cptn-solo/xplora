using Assets.Scripts.ECS;
using Assets.Scripts.Data;
using UnityEngine;
using Leopotam.EcsLite;
using System;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleUnit : BaseEntityView<Hero>        
    {        
        [SerializeField] private HeroAnimation heroAnimation;

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

        public override void Visualize<V>(V visualInfo, EcsPackedEntityWithWorld visualEntity)
        {
            if (heroAnimation != null)
                heroAnimation.Visualize(() => base.Visualize(visualInfo, visualEntity), visualInfo);
            else
                base.Visualize(visualInfo, visualEntity);
        }
    }

}