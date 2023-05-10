using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RaidInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<RaidComp> raidPool = default;
        private readonly EcsPoolInject<UpdateAssetBalanceTag> updateBalancePool = default;

        private readonly EcsCustomInject<RaidService> raidService = default;
        private readonly EcsCustomInject<WorldService> worldService = default;

        public void Init(IEcsSystems systems)
        {
            var success = raidService.Value.AssignPlayerAndEnemies(
                out var playerHeroes,
                out var opponentHeroes);

            var raidEntity = ecsWorld.Value.NewEntity();

            ref var raidComp = ref raidPool.Value.Add(raidEntity);
            raidComp.PlayerLibHeroInstances = playerHeroes;
            raidComp.OpponentLibHeroInstances = opponentHeroes;

            raidComp.Assets = new Asset[]
            {
                new Asset()
                {
                    AssetType = AssetType.Money,
                    IconCode = BundleIcon.SnowFlake,
                    Count = 0
                }
            };

            updateBalancePool.Value.Add(raidEntity);

            raidService.Value.RaidEntity = ecsWorld.Value.PackEntity(raidEntity);

            worldService.Value.ResetPoiUsageStatus();
        }
    }
}