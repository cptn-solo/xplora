using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAutoMakeTurnSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<MakeTurnTag> makeTurnTagPool;
        private readonly EcsPoolInject<ReadyTurnTag> readyTurnTagPool;

        private readonly EcsFilterInject<Inc<BattleInfo, BattleInProgressTag>> battleFilter;
        private readonly EcsFilterInject<Inc<BattleTurnInfo, ReadyTurnTag>> turnInfoFilter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            if (battleFilter.Value.GetEntitiesCount() == 0)
                return;

            var autoplay = battleService.Value.PlayMode switch
            {
                BattleMode.Autoplay => true,
                BattleMode.Fastforward => true,
                _ => false
            };

            if (!autoplay)
                return;

            foreach (var entity in turnInfoFilter.Value)
            {
                makeTurnTagPool.Value.Add(entity);
                readyTurnTagPool.Value.Del(entity);
            }

        }
    }
}
