using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleCompleteTurnSystem : BaseEcsSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<MakeTurnTag> makeTurnTagPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<CompletedTurnTag> completeTagPool = default;
        private readonly EcsPoolInject<ScheduleVisualsTag> scheduleVisualsTagPool = default;

        private readonly EcsPoolInject<LethalTag> lethalTagPool = default;
        private readonly EcsPoolInject<DeadTag> deadTagPool = default;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, MakeTurnTag>> makeTurnFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach(var entity in makeTurnFilter.Value)
            {
                CompleteTurn(entity);
                makeTurnTagPool.Value.Del(entity);

                // if not Fast Forward
                if (battleService.Value.PlayMode != BattleMode.Fastforward)
                   scheduleVisualsTagPool.Value.Add(entity);
            }
        }

        private void CompleteTurn(int turnEntity)
        {
            MarkDeadHero<AttackerRef>(turnEntity); // lethal for attacker is
                                                          // applicable only while
                                                          // applying queued effects
            MarkDeadHero<TargetRef>(turnEntity);

            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);

            battleService.Value.NotifyTurnEventListeners(); // inprogress - will queue
                                                            // attack / damage
                                                            // animations
            
            turnInfo.State = TurnState.TurnCompleted;
            completeTagPool.Value.Add(turnEntity);

            battleService.Value.NotifyTurnEventListeners(); // sums up the turn aftermath
        }

        private bool MarkDeadHero<T>(int turnEntity) where T : struct, IPackedWithWorldRef
        {
            var pool = ecsWorld.Value.GetPool<T>();
            if (!pool.Has(turnEntity))
                return false;

            ref var subjectRef = ref pool.Get(turnEntity);
            if (!subjectRef.Packed.Unpack(out _, out var subjectEntity))
                throw new Exception("No hero instance");

            if (!lethalTagPool.Value.Has(subjectEntity))
                return false;

            if (!deadTagPool.Value.Has(subjectEntity))
                deadTagPool.Value.Add(subjectEntity);

            return true;
        }


    }
}
