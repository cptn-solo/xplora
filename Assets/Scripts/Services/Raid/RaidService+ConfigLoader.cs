using Assets.Scripts.Data;
using UnityEngine.Events;
using Zenject;

namespace Assets.Scripts.Services
{
    public partial class RaidService : IConfigLoaderService // Config loading
    {
        [Inject] private readonly StreamingAssetsLoaderService saLoader = default;

        private EnemySpawnRulesLibrary spawnRulesLibrary = EnemySpawnRulesLibrary.EmptyLibrary();
        private EnemySpawnRulesConfigLoader enemySpawnRulesConfigLoader;

        public EnemySpawnRulesLibrary EnemySpawnRulesLibrary => spawnRulesLibrary;

        public event UnityAction OnDataAvailable;

        public bool DataAvailable =>
            enemySpawnRulesConfigLoader != null &&
            enemySpawnRulesConfigLoader.DataAvailable;

        public void InitConfigLoading()
        {
            enemySpawnRulesConfigLoader = new(spawnRulesLibrary, NotifyIfAllDataAvailable);
        }

        public void NotifyIfAllDataAvailable()
        {
            if (DataAvailable)
                OnDataAvailable?.Invoke();
        }

        public void LoadCachedData()
        {
            saLoader.LoadData(enemySpawnRulesConfigLoader.ConfigFileName, enemySpawnRulesConfigLoader.ProcessSerializedString);
        }

        public void LoadRemoteData()
        {
            enemySpawnRulesConfigLoader.LoadGoogleData();
        }

    }

}

