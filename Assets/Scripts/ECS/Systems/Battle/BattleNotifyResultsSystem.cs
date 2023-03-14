using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleNotifyResultsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<DelayTimerComp<WinnerTag>> delayPool = default;
        private readonly EcsFilterInject<Inc<DelayTimerComp<WinnerTag>>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

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
