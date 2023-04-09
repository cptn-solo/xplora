using Assets.Scripts.Data;
using UnityEngine.Events;
using Zenject;

namespace Assets.Scripts.Services
{
    public partial class HeroLibraryService : IConfigLoaderService // Config Loader
    {
        [Inject] private readonly StreamingAssetsLoaderService saLoader = default;

        private HeroesConfigLoader heroesConfigLoader;// loads directly to ecs world so no library/config

        private DamageTypesLibrary damageTypesLib = DamageTypesLibrary.EmptyLibrary();
        private DamageTypesConfigLoader damageConfigLoader;
        public DamageTypesLibrary DamageTypesLibrary => damageTypesLib;
        
        private HeroRelationsConfig heroRelationsConfig = new();
        private HeroRelationRulesConfigLoader relationConfigLoader;
        public ref HeroRelationsConfig HeroRelationsConfigProcessor() => ref heroRelationsConfig;

        private HeroRelationEffectsLibrary relationEffectsLib = HeroRelationEffectsLibrary.EmptyLibrary();
        private HeroRelationEffectsLibraryLoader relationEffectsLibraryLoader;
        public ref HeroRelationEffectsLibrary HeroRelationEffectsLibProcessor() => ref relationEffectsLib;

        public event UnityAction OnDataAvailable;
        public bool DataAvailable =>
            heroesConfigLoader != null &&
            damageConfigLoader != null &&
            relationConfigLoader != null &&
            relationEffectsLibraryLoader != null &&
            heroesConfigLoader.DataAvailable &&
            damageConfigLoader.DataAvailable &&
            relationConfigLoader.DataAvailable &&
            relationEffectsLibraryLoader.DataAvailable;

        public void InitConfigLoading()
        {
            heroesConfigLoader = new(HeroConfigProcessor, NotifyIfAllDataAvailable);
            damageConfigLoader = new(DamageTypesLibrary, NotifyIfAllDataAvailable);
            relationConfigLoader = new(HeroRelationsConfigProcessor, NotifyIfAllDataAvailable);
            relationEffectsLibraryLoader = new(HeroRelationEffectsLibProcessor, NotifyIfAllDataAvailable);
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
            saLoader.LoadData(relationConfigLoader.ConfigFileName, relationConfigLoader.ProcessSerializedString);
            saLoader.LoadData(relationEffectsLibraryLoader.ConfigFileName, relationEffectsLibraryLoader.ProcessSerializedString);
        }

        public void LoadRemoteData()
        {
            heroesConfigLoader.LoadGoogleData();
            damageConfigLoader.LoadGoogleData();
            relationConfigLoader.LoadGoogleData();
            relationEffectsLibraryLoader.LoadGoogleData();
        }
    }
}
