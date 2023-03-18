using Assets.Scripts.Data;
using UnityEngine.Events;
using Zenject;

namespace Assets.Scripts.Services
{
    public partial class HeroLibraryService : IConfigLoaderService // Config Loader
    {
        [Inject] private readonly StreamingAssetsLoaderService saLoader = default;

        private HeroesConfigLoader heroesConfigLoader;

        private DamageTypesLibrary damageTypesLib = DamageTypesLibrary.EmptyLibrary();
        private DamageTypesConfigLoader damageConfigLoader;

        public event UnityAction OnDataAvailable;
        public bool DataAvailable =>
            heroesConfigLoader != null &&
            damageConfigLoader != null &&
            heroesConfigLoader.DataAvailable &&
            damageConfigLoader.DataAvailable;

        public void InitConfigLoading()
        {
            heroesConfigLoader = new(ProcessEcsHeroConfig, NotifyIfAllDataAvailable);
            damageConfigLoader = new(DamageTypesLibrary, NotifyIfAllDataAvailable);
        }

        public void NotifyIfAllDataAvailable()
        {
            if (DataAvailable)
                OnDataAvailable?.Invoke();
        }

        public void LoadCachedData()
        {
            saLoader.LoadData(heroesConfigLoader.ConfigFileName, heroesConfigLoader.ProcessSerializedString);
            saLoader.LoadData(damageConfigLoader.ConfigFileName, damageConfigLoader.ProcessSerializedString);
        }

        public void LoadRemoteData()
        {
            heroesConfigLoader.LoadGoogleData();
            damageConfigLoader.LoadGoogleData();
        }
    }
}
