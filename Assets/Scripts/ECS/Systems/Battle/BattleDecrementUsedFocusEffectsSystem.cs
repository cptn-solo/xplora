using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{

    public partial class BattleDecrementUsedFocusEffectsSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EffectFocusComp> pool = default;
        private readonly EcsPoolInject<DecrementPendingTag> decrementPool = default;
        private readonly EcsFilterInject<Inc<UsedFocusEntityTag, DecrementPendingTag, EffectFocusComp>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var focus = ref pool.Value.Get(entity);
                if (!focus.EffectEntity.Unpack(out var world, out var effectEntity))
                    throw new Exception("Stale effect entity");

                world.UseRelationEffect(effectEntity);
                decrementPool.Value.Del(entity);
            }
        }

    }
}
