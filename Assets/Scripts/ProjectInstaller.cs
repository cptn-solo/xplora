using Assets.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Assets.Scripts
{

    public class ProjectInstaller : MonoInstaller, IInitializable
    {
        [SerializeField] private MenuNavigationService menuNavigationService;

        public override void InstallBindings()
        {
            Container.Bind<MenuNavigationService>().FromInstance(menuNavigationService).AsSingle();
            BindInstallerInterfaces();
        }

        public void Initialize()
        {
            
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