using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleNotifyResultsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<DelayTimerComp<WinnerTag>> delayPool;
        private readonly EcsFilterInject<Inc<DelayTimerComp<WinnerTag>>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var delayComp = ref delayPool.Value.Get(entity);
                if (!delayComp.Ready)
                    continue;

                battleService.Value.NotifyBattleEventListeners();

                delayPool.Value.Del(entity);
            }
        }

    }
}
