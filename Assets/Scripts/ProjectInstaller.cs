using UnityEngine;
using Zenject;

namespace Assets.Scripts
{

    public class ProjectInstaller : MonoInstaller, IInitializable
    {
        [SerializeField] private MenuNavigationService menuNavigationService;
        [SerializeField] private TeamManagementService teamManagementService;
        [SerializeField] private HeroLibraryManagementService libManagementService;
        public override void InstallBindings()
        {
            Container
                .Bind<MenuNavigationService>()
                .FromInstance(menuNavigationService).AsSingle();

            Container
                .Bind<TeamManagementService>()
                .FromInstance(teamManagementService).AsSingle();

            Container
                .Bind<HeroLibraryManagementService>()
                .FromInstance(libManagementService).AsSingle();

            BindInstallerInterfaces();
        }

        public void Initialize()
        {
            teamManagementService.LoadData();
            libManagementService.LoadData();
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