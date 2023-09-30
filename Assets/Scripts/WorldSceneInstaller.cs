using Assets.Scripts.Services;
using Assets.Scripts.UI.Common;
using UnityEngine;
using Zenject;

namespace Assets.Scripts
{
    public class WorldSceneInstaller : MonoInstaller, IInitializable
    {
        [SerializeField] private GameObject heroKindBarPrefab;
        
        private RaidService raidService;

        [Inject]
        public void Construct(RaidService raidService)
        {
            this.raidService = raidService;
        }
        
        public void Initialize()
        {
            raidService.HeroKindBarFactory = Container.Resolve<HeroKindBar.Factory>();
        }

        public override void InstallBindings()
        {
            Container
                .BindFactory<HeroKindBar, HeroKindBar.Factory>()
                .FromComponentInNewPrefab(heroKindBarPrefab);

            BindInstallerInterfaces();
        }

        private void BindInstallerInterfaces()
        {
            Container
                .BindInterfacesTo<WorldSceneInstaller>()
                .FromInstance(this)
                .AsSingle();
        }
    }
}