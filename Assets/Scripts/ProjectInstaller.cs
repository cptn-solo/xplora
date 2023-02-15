using Assets.Scripts.Services;
using Assets.Scripts.Services.App;
using UnityEngine;
using Zenject;

namespace Assets.Scripts
{
    public class ProjectInstaller : MonoInstaller, IInitializable
    {
        [SerializeField] private PlayerPreferencesService playerPrefsService;
        [SerializeField] private AudioPlaybackService audioPlaybackService;
        [SerializeField] private StreamingAssetsLoaderService saLoaderService;
        [SerializeField] private MenuNavigationService menuNavigationService;
        [SerializeField] private HeroLibraryService libManagementService;
        [SerializeField] private BattleManagementService battleManagementService;
        [SerializeField] private WorldService worldService;
        [SerializeField] private RaidService raidService;

        public override void InstallBindings()
        {
            Container
                .Bind<PlayerPreferencesService>()
                .FromInstance(playerPrefsService).AsSingle();

            Container
                .Bind<AudioPlaybackService>()
                .FromInstance(audioPlaybackService).AsSingle();

            Container
                .Bind<StreamingAssetsLoaderService>()
                .FromInstance(saLoaderService).AsSingle();

            Container
                .Bind<MenuNavigationService>()
                .FromInstance(menuNavigationService).AsSingle();

            Container
                .Bind<HeroLibraryService>()
                .FromInstance(libManagementService).AsSingle();

            Container
                .Bind<BattleManagementService>()
                .FromInstance(battleManagementService).AsSingle();

            Container
                .Bind<WorldService>()
                .FromInstance(worldService).AsSingle();

            Container
                .Bind<RaidService>()
                .FromInstance(raidService).AsSingle();

            BindInstallerInterfaces();
        }

        public void Initialize()
        {
            libManagementService.Init();
            libManagementService.LoadData();

            battleManagementService.Init(
                playerPrefsService,
                libManagementService,
                menuNavigationService,
                raidService);
            
            audioPlaybackService.Init(
                menuNavigationService);
            
            raidService.Init(
                menuNavigationService,
                libManagementService,
                worldService);

            worldService.Init(
                menuNavigationService);
            worldService.LoadData();
        }

        private void BindInstallerInterfaces()
        {
            Container
                .BindInterfacesTo<ProjectInstaller>()
                .FromInstance(this)
                .AsSingle();
        }
    }
}