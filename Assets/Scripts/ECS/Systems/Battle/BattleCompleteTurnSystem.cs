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
            MarkDeadHero<AttackerRef>(turnEntity);
            MarkDeadHero<TargetRef>(turnEntity);

            battleService.Value.NotifyTurnEventListeners(); // sums up the turn aftermath

            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            turnInfo.State = TurnState.TurnCompleted;

            completeTagPool.Value.Add(turnEntity);
        }

        private void MarkDeadHero<T>(int turnEntity) where T : struct, IPackedWithWorldRef
        {
            ref var heroInstanceRef = ref ecsWorld.Value.GetPool<T>().Get(turnEntity);
            if (!heroInstanceRef.Packed.Unpack(out var world, out var heroInstanceEntity))
                throw new Exception("No hero instance");

            ref var hpComp = ref hpCompPool.Value.Get(heroInstanceEntity);
            if (hpComp.HP <= 0)
                deadTagPool.Value.Add(turnEntity);
        }


    }
}
