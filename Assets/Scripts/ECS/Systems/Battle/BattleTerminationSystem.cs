using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleTerminationSystem : IEcsPostRunSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsPoolInject<DelayTimerComp<DestroyTag>> delayPool;

        private readonly EcsFilterInject<
            Inc<BattleInfo, DelayTimerComp<DestroyTag>>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void PostRun(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                if (delayPool.Value.Has(entity))
                {
                    ref var delayComp = ref delayPool.Value.Get(entity);
                    if (!delayComp.Ready)
                        continue;

                    delayPool.Value.Del(entity);
                }

                TerminateBattle(entity);
            }
        }

        private void TerminateBattle(int entity)
        {
            ref var battleInfo = ref battleInfoPool.Value.Get(entity);

            battleService.Value.NotiifyBattleComplete(
                battleInfo.WinnerTeamId == battleInfo.PlayerTeam.Id,
                battleInfo.PotAssets);
        }
    }
}
