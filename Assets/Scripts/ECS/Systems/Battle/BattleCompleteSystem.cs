using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleCompleteSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInProgressTag> battleInProgressTagPool;
        private readonly EcsPoolInject<BattleCompletedTag> battleCompletedTagPool;
        private readonly EcsPoolInject<DestroyTag> destroyTagPool;
        private readonly EcsPoolInject<DelayTimerComp<WinnerTag>> delayWinnerPool;
        private readonly EcsPoolInject<DelayTimerComp<DestroyTag>> delayDestroyPool;
        private readonly EcsPoolInject<WinnerTag> winnerTagPool;
        private readonly EcsPoolInject<RetreatTag> retreatTagPool;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsFilterInject<
            Inc<BattleInfo, WinnerTag>,
            Exc<BattleCompletedTag>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                CompleteBattle(entity);
        }

        private void CompleteBattle(int battleEntity)
        {
            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);

            battleInfo.SetState(BattleState.Completed);
            battleCompletedTagPool.Value.Add(battleEntity);

            destroyTagPool.Value.Add(battleEntity);

            ref var delayWinner = ref delayWinnerPool.Value.Add(battleEntity);
            delayWinner.SetDelayFromNow(1.5f);

            ref var delayDestroy = ref delayDestroyPool.Value.Add(battleEntity);
            delayDestroy.SetDelayFromNow(3f);
        }
    }
}
