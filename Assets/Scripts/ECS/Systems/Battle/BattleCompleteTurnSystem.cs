using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleCompleteTurnSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<MakeTurnTag> makeTurnTagPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<CompletedTurnTag> completeTagPool = default;

        private readonly EcsPoolInject<DeadTag> deadTagPool = default;

        private readonly EcsPoolInject<IntValueComp<HpTag>> hpCompPool = default;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, MakeTurnTag>> makeTurnFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            foreach(var entity in makeTurnFilter.Value)
            {
                CompleteTurn(entity);
                makeTurnTagPool.Value.Del(entity);
            }
        }

        private void CompleteTurn(int turnEntity)
        {
            MarkDeadHero<AttackerRef>(turnEntity); // lethal for attacker is
                                                          // applicable only while
                                                          // applying queued effects
            bool lethal = MarkDeadHero<TargetRef>(turnEntity);

            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            turnInfo.Lethal = lethal;
            //turnInfo.HealthCurrent = hp;

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

            ref var heroInstanceRef = ref pool.Get(turnEntity);
            if (!heroInstanceRef.Packed.Unpack(out var world, out var heroInstanceEntity))
                throw new Exception("No hero instance");

            ref var hpComp = ref hpCompPool.Value.Get(heroInstanceEntity);
            if (hpComp.Value > 0)
                return false;

            deadTagPool.Value.Add(heroInstanceEntity);

            return true;
        }


    }
}
