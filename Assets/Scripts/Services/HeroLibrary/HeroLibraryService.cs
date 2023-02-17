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
        public Hero[] PlayerHeroes => GetEcsTeamHeroes(PlayerTeamEntity, true);
        public Hero[] EnemyHeroes => GetEcsTeamHeroes(EnemyTeamEntity, true);
        public Hero[] NonPlayerTeamHeroes => GetEcsNotInTeamHeroes(PlayerTeamEntity, true);

        public bool PrepareTeamsForBattle(out Hero[] playerHeroes, out Hero[] enemyHeroes)
        {
            ResetHealthCurrent();

            playerHeroes = PlayerHeroes;
            enemyHeroes = EnemyHeroes;

            if (playerHeroes.Length > 0 && enemyHeroes.Length > 0)
                return true;

            return false;
        }

        public void Init()
        {
            StartEcsContext();
            InitConfigLoading();
        }

        internal void ResetHealthCurrent() =>
            ResetEcsHealthAndEffects();

        internal void ResetTeams()
        {
            ResetEcsTeamAssets();
            ResetEcsHealthAndEffects();
        }

        internal void RetireHero(Hero hero) =>
            RetireEcsHero(hero);
        
        internal void MoveToEnemyFrontLine(Hero hero) =>
            MoveEcsEnemyFrontLine(hero);

        internal Hero HeroById(int heroId) =>
            GetEcsHeroById(heroId);

        internal Hero HeroAtPosition(Tuple<int, BattleLine, int> position) =>
            GetEcsHeroAtPosition(position);

        internal void MoveHero(Hero hero, Tuple<int, BattleLine, int> pos) =>
            MoveEcsHeroToPosition(hero, pos);

        internal void UpdateHero(Hero target) =>
            UpdateEcsHero(target);
    }
}