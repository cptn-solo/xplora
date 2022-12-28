using Assets.Scripts.UI.Data;
using UnityEngine;
using Assets.Scripts.Services.ConfigDataManagement.Parsers;
using Zenject;
using Assets.Scripts.Services.App;
using UnityEngine.Events;

namespace Assets.Scripts.Services
{
    public class HeroLibraryManagementService : MonoBehaviour
    {
        [Inject] private readonly StreamingAssetsLoaderService saLoader;

        private HeroesLibrary library = HeroesLibrary.EmptyLibrary();
        private HeroesConfigLoader heroesConfigLoader;
        public HeroesLibrary Library => library;

        private DamageTypesLibrary damageTypesLib = DamageTypesLibrary.EmptyLibrary();
        private DamageTypesConfigLoader damageConfigLoader;
        public DamageTypesLibrary DamageTypesLibrary => damageTypesLib;

        public Team PlayerTeam => library.PlayerTeam;
        public Team EnemyTeam => library.EnemyTeam;

        public event UnityAction OnDataAvailable;
        public bool DataAvailable => 
            heroesConfigLoader != null &&
            damageConfigLoader != null &&
            heroesConfigLoader.DataAvailable &&
            damageConfigLoader.DataAvailable;

        private void Awake()
        {
            heroesConfigLoader = new(Library, NotifyIfAllDataAvailable);
            damageConfigLoader = new(DamageTypesLibrary, NotifyIfAllDataAvailable);
        }

        private void NotifyIfAllDataAvailable()
        {
            if (DataAvailable)
                OnDataAvailable?.Invoke();
        }

        public bool PrepareTeamsForBattle(out Hero[] playerHeroes, out Hero[] enemyHeroes)
        {
            ResetHealthCurrent();

            playerHeroes = library.TeamHeroes(PlayerTeam.Id, true);
            enemyHeroes = library.TeamHeroes(EnemyTeam.Id, true);

            if (playerHeroes.Length > 0 && enemyHeroes.Length > 0)
                return true;

            return false;
        }

        public void LoadData()
        {
            saLoader.LoadData("Heroes.json", heroesConfigLoader.ProcessSerializedString);
            saLoader.LoadData("DamageTypes.json", damageConfigLoader.ProcessSerializedString);
        }

        public void LoadGoogleData()
        {
            heroesConfigLoader.LoadGoogleData();
            damageConfigLoader.LoadGoogleData();
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