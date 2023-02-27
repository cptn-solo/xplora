using UnityEngine;
using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System;
using Assets.Scripts.Battle;
using Assets.Scripts.UI.Data;

namespace Assets.Scripts.Services
{
    public partial class HeroLibraryService : MonoBehaviour
    {
        private MenuNavigationService menuNavigationService;

        public DamageTypesLibrary DamageTypesLibrary => damageTypesLib;

        public ref Team PlayerTeam => ref GetEcsPlayerTeam();
        public ref Team EnemyTeam => ref GetEcsEnemyTeam();
        public EcsPackedEntityWithWorld[] PlayerHeroes => GetEcsTeamHeroes(PlayerTeamEntity);
        public EcsPackedEntityWithWorld[] EnemyHeroes => GetEcsTeamHeroes(EnemyTeamEntity);
        public EcsPackedEntityWithWorld[] NonPlayerTeamHeroes => GetEcsNotInTeamHeroes(PlayerTeamEntity, true);

        public void Init(MenuNavigationService menuNavigationService)
        {
            StartEcsContext();
            InitConfigLoading();

            this.menuNavigationService = menuNavigationService;
            menuNavigationService.OnBeforeNavigateToScreen += MenuNavigationService_OnBeforeNavigateToScreen;
        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens current, Screens prev)
        {
            if (prev == Screens.HeroesLibrary)
                UnlinkCardRefs();            
        }

        private void OnDestroy()
        {
            StopEcsContext();
            menuNavigationService.OnBeforeNavigateToScreen -= MenuNavigationService_OnBeforeNavigateToScreen;
        }
        
        internal EcsPackedEntityWithWorld? HeroAtPosition(Tuple<int, BattleLine, int> position) =>
            GetEcsHeroAtPosition(position);

        internal void MoveHero(EcsPackedEntityWithWorld hero, Tuple<int, BattleLine, int> pos) =>
            MoveEcsHeroToPosition(hero, pos);

        internal EntityViewFactory<Hero> HeroCardFactory { get; set; } 
    }
}