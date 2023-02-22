using UnityEngine;
using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System;

namespace Assets.Scripts.Services
{
    public partial class HeroLibraryService : MonoBehaviour
    {
        public DamageTypesLibrary DamageTypesLibrary => damageTypesLib;

        public ref Team PlayerTeam => ref GetEcsPlayerTeam();
        public ref Team EnemyTeam => ref GetEcsEnemyTeam();
        public Hero[] PlayerHeroes => GetEcsTeamHeroes(PlayerTeamEntity);
        public Hero[] EnemyHeroes => GetEcsTeamHeroes(EnemyTeamEntity);
        public Hero[] NonPlayerTeamHeroes => GetEcsNotInTeamHeroes(PlayerTeamEntity, true);

        public void Init()
        {
            StartEcsContext();
            InitConfigLoading();
        }

        private void OnDestroy()
        {
            StopEcsContext();
        }

        internal void RetireHero(Hero hero) =>
            RetireEcsHero(hero);
        
        internal void MoveToEnemyFrontLine(Hero hero) =>
            MoveEcsEnemyFrontLine(hero);

        internal Hero HeroById(int heroId) =>
            GetEcsHeroById(heroId);

        internal EcsPackedEntityWithWorld? HeroAtPosition(Tuple<int, BattleLine, int> position) =>
            GetEcsHeroAtPosition(position);

        internal void MoveHero(EcsPackedEntityWithWorld hero, Tuple<int, BattleLine, int> pos) =>
            MoveEcsHeroToPosition(hero, pos);
    }
}