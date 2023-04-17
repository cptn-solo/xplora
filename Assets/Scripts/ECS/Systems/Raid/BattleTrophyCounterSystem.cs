using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleTrophyCounterSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<BattleAftermathComp> aftermathPool = default;
        private readonly EcsPoolInject<RaidComp> raidPool = default;
        private readonly EcsPoolInject<UpdateAssetBalanceTag> updatePool = default;

        private readonly EcsFilterInject<Inc<BattleComp, BattleAftermathComp>> aftermathFilter = default;

        private readonly EcsCustomInject<RaidService> raidService = default;

        public void Run(IEcsSystems systems)
        {
            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                return;

            foreach (var battleEntity in aftermathFilter.Value)
            {
                ref var aftermathComp = ref aftermathPool.Value.Get(battleEntity);
                if (aftermathComp.Won)
                {
                    ref var raidComp = ref raidPool.Value.Get(raidEntity);

                    var buffer = ListPool<Asset>.Get();
                    buffer.AddRange(raidComp.Assets);

                    foreach (var trophy in aftermathComp.Trophy)
                    {
                        if (buffer.FindIndex(x => x.AssetType == trophy.AssetType) is var idx &&
                            idx >= 0)
                        {
                            var val = buffer[idx];
                            val.Count += trophy.Count;
                            buffer[idx] = val;
                        }
                        else
                        {
                            buffer.Add(new Asset()
                            {
                                AssetType = trophy.AssetType,
                                Count = trophy.Count,
                                Id = trophy.Id,
                            });
                        }
                    }

                    raidComp.Assets = buffer.ToArray();

                    ListPool<Asset>.Add(buffer);

                    if (!updatePool.Value.Has(raidEntity))
                        updatePool.Value.Add(raidEntity);
                }
            }
        }
    }
}