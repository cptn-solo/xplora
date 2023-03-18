using Assets.Scripts.Data;
using UnityEngine.Events;
using Zenject;

namespace Assets.Scripts.Services
{
    public partial class RaidService : IConfigLoaderService // Config loading
    {
        [Inject] private readonly StreamingAssetsLoaderService saLoader = default;

        private OpponentSpawnConfig opponentSpawnConfig = new();
        private OpponentTeamMemberSpawnConfig teamSpawnConfig = new();

        private EnemySpawnRulesConfigLoader opponentSpawnConfigLoader;
        private TeamSpawnRulesConfigLoader teamSpawnConfigLoader;

        public ref OpponentSpawnConfig OpponentSpawnConfig() =>
            ref opponentSpawnConfig;

        public ref OpponentTeamMemberSpawnConfig OpponentTeamMemberSpawnConfig() =>
            ref teamSpawnConfig;
        
        public event UnityAction OnDataAvailable;

        public bool DataAvailable =>
            opponentSpawnConfigLoader != null &&
            teamSpawnConfigLoader != null &&
            opponentSpawnConfigLoader.DataAvailable &&
            teamSpawnConfigLoader.DataAvailable;

        public void InitConfigLoading()
        {
            opponentSpawnConfigLoader = new(OpponentSpawnConfig, NotifyIfAllDataAvailable);
            teamSpawnConfigLoader = new(OpponentTeamMemberSpawnConfig, NotifyIfAllDataAvailable);
        }

        public void NotifyIfAllDataAvailable()
        {
            if (DataAvailable)
                OnDataAvailable?.Invoke();
        }

        public void LoadCachedData()
        {
            saLoader.LoadData(opponentSpawnConfigLoader.ConfigFileName, opponentSpawnConfigLoader.ProcessSerializedString);
            saLoader.LoadData(teamSpawnConfigLoader.ConfigFileName, teamSpawnConfigLoader.ProcessSerializedString);
        }

        public void LoadRemoteData()
        {
            opponentSpawnConfigLoader.LoadGoogleData();
            teamSpawnConfigLoader.LoadGoogleData();
        }

    }

}

