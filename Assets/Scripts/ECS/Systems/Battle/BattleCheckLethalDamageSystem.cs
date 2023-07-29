using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleCheckLethalDamageSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<LethalTag> lethalTagPool = default;
        private readonly EcsPoolInject<AttackTag> attackTagPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, AttackerRef>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var attackerRef = ref attackerRefPool.Value.Get(entity);
                if (!attackerRef.Packed.Unpack(out var world, out var attackerEntity))
                    throw new Exception("Stale Attacker");

                if (!lethalTagPool.Value.Has(attackerEntity))
                    continue;

                attackTagPool.Value.Del(entity);
            }
        }
    }
}
