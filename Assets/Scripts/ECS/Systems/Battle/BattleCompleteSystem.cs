using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleCompleteSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<BattleCompletedTag> battleCompletedTagPool = default;
        private readonly EcsPoolInject<DestroyTag> destroyTagPool = default;
        private readonly EcsPoolInject<DelayTimerComp<WinnerTag>> delayWinnerPool = default;
        private readonly EcsPoolInject<DelayTimerComp<DestroyTag>> delayDestroyPool = default;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsFilterInject<
            Inc<BattleInfo, WinnerTag>,
            Exc<BattleCompletedTag>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
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
