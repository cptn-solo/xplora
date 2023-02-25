using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Google.Apis.Sheets.v4.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleCompleteTurnSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<MakeTurnTag> makeTurnTagPool;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool;
        private readonly EcsPoolInject<CompletedTurnTag> completeTagPool;

        private readonly EcsPoolInject<AttackerRef> attackerRefPool;
        private readonly EcsPoolInject<TargetRef> targetRefPool;
        private readonly EcsPoolInject<DeadTag> deadTagPool;

        private readonly EcsPoolInject<HPComp> hpCompPool;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, MakeTurnTag>> makeTurnFilter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

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
            turnInfo.State = TurnState.TurnCompleted;
            turnInfo.Lethal = lethal;
            //turnInfo.HealthCurrent = hp;

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

            deadTagPool.Value.Add(turnEntity);

            return true;
        }


    }
}
