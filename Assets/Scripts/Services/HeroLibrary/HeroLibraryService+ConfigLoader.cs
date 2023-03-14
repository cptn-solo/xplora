using Assets.Scripts.Data;
using UnityEngine.Events;
using Zenject;

namespace Assets.Scripts.Services
{
    public partial class HeroLibraryService // Config Loader
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

        private void InitConfigLoading()
        {
            heroesConfigLoader = new(ProcessEcsHeroConfig, NotifyIfAllDataAvailable);
            damageConfigLoader = new(DamageTypesLibrary, NotifyIfAllDataAvailable);
        }

        public void NotifyIfAllDataAvailable()
        {
            if (DataAvailable)
                OnDataAvailable?.Invoke();
        }

        public void LoadData()
        {
            saLoader.LoadData(heroesConfigLoader.ConfigFileName, heroesConfigLoader.ProcessSerializedString);
            saLoader.LoadData(damageConfigLoader.ConfigFileName, damageConfigLoader.ProcessSerializedString);
        }

        public void LoadGoogleData()
        {
            heroesConfigLoader.LoadGoogleData();
            damageConfigLoader.LoadGoogleData();
        }
    }
}
