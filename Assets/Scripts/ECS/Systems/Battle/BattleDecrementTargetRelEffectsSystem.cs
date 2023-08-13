using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public partial class BattleDecrementTargetRelEffectsSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EffectInstanceInfo> pool = default;
        private readonly EcsPoolInject<TargetRef> subjectPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsFilterInject<Inc<BattleTurnInfo, CompletedTurnTag, AttackerRef>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var subjectRef = ref subjectPool.Value.Get(entity);
                if (!subjectRef.Packed.Unpack(out var world, out var subjectEntity))
                    throw new Exception("Stale subject entity");

                ref var turnInfo = ref turnInfoPool.Value.Get(entity);

                var en = world.SubjectEffectsEntities(subjectEntity);
                while (en.MoveNext())
                {
                    ref var effect = ref pool.Value.Get(en.Current);

                    //TODO: add effect type condition maybe, so the effects for defence should not be removed untill used
                    if (effect.StartTurn < turnInfo.Turn)
                        effect.UsageLeft--;
                }
            }
        }

    }
}
