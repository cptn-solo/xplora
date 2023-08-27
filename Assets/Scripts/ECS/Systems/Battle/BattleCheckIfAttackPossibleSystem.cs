using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// If C tag present on the T subject (attacker or target) then 
    /// stop attack by removing AttackTag from the turn entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="C"></typeparam>
    public class BattleCheckIfAttackPossibleSystem<T, C> : BaseEcsSystem 
        where T : struct, IPackedWithWorldRef
        where C : struct
    {
        private readonly EcsPoolInject<T> subjectRefPool = default;
        private readonly EcsPoolInject<C> conditionTagPool = default;
        private readonly EcsPoolInject<AttackTag> attackTagPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, T>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var subjectRef = ref subjectRefPool.Value.Get(entity);
                if (!subjectRef.Packed.Unpack(out var world, out var subjectEntity))
                    throw new Exception("Stale Attacker");

                if (conditionTagPool.Value.Has(subjectEntity))
                    attackTagPool.Value.Del(entity);             
            }
        }
    }
}
