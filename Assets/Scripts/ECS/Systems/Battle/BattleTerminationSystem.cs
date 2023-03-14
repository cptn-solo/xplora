using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleTerminationSystem : IEcsPostRunSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<BattlePotComp> battlePotPool = default;

        private readonly EcsPoolInject<DelayTimerComp<DestroyTag>> delayPool = default;

        private readonly EcsFilterInject<
            Inc<BattleInfo, BattlePotComp, DelayTimerComp<DestroyTag>>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

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
            ref var battlePot = ref battlePotPool.Value.Get(entity);
            battleService.Value.NotiifyBattleComplete(
                battleInfo.WinnerTeamId == battleInfo.PlayerTeam.Id,
                battlePot.PotAssets);
        }
    }
}
