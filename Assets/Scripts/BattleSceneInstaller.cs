using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts
{

    public class BattleSceneInstaller : MonoInstaller, IInitializable
    {
        [SerializeField] private GameObject bundleIconHostPrefab;

        private BattleManagementService battleManager;

        [Inject]        
        public void Construct(BattleManagementService battleManager)
        {
            this.battleManager = battleManager;
        }

        public void Initialize()
        {
            battleManager.BundleIconFactory = Container.Resolve<BundleIconHost.Factory>();
        }

        public override void InstallBindings()
        {
            Container
                .BindFactory<BundleIconHost, BundleIconHost.Factory>()
                .FromComponentInNewPrefab(bundleIconHostPrefab);

            BindInstallerInterfaces();
        }
        
        private void BindInstallerInterfaces()
        {
            Container
                .BindInterfacesTo<BattleSceneInstaller>()
                .FromInstance(this)
                .AsSingle();
        }
    }
}