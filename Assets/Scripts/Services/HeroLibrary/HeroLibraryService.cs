using UnityEngine;
using Assets.Scripts.Data;

namespace Assets.Scripts.Services
{    
    public partial class HeroLibraryService : MonoBehaviour
    {

        public HeroesLibrary Library => library;

        public DamageTypesLibrary DamageTypesLibrary => damageTypesLib;

        public Team PlayerTeam => library.PlayerTeam;
        public Team EnemyTeam => library.EnemyTeam;

        public bool PrepareTeamsForBattle(out Hero[] playerHeroes, out Hero[] enemyHeroes)
        {
            ResetHealthCurrent();

            playerHeroes = library.TeamHeroes(PlayerTeam.Id, true);
            enemyHeroes = library.TeamHeroes(EnemyTeam.Id, true);

            if (playerHeroes.Length > 0 && enemyHeroes.Length > 0)
                return true;

            return false;
        }

        public void Init()
        {
            InitConfigLoading();
        }

        internal void ResetHealthCurrent() =>
            library.ResetHealthAndEffects();

        internal void ResetTeams()
        {
            library = Library.ResetTeamAssets();
            Library.ResetHealthAndEffects();
        }
    }
}