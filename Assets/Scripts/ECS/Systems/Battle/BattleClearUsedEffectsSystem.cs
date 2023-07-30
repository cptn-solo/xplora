using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearUsedEffectsSystem<T> : BaseEcsSystem where T : struct
    {
        private readonly EcsPoolInject<ProcessedHeroTag> processedPool = default;
        private readonly EcsPoolInject<ActiveEffectComp> pool = default;

        private readonly EcsFilterInject<Inc<ActiveEffectComp, T>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            foreach (var entity in filter.Value)
            {
                ref var effect = ref pool.Value.Get(entity);

                if (!effect.Subject.Unpack(out _, out var subject))
                    throw new Exception("Stale subject entity");

                if (!processedPool.Value.Has(subject))
                    continue;

                ExtraProcess(world, ref effect, subject);

                world.UseEffect(entity);
            }
        }
        /// <summary>
        /// can be moved to a subclass one day
        /// </summary>
        /// <param name="world">effect's world</param>
        /// <param name="effect">effect to be remved</param>
        /// <param name="subject">Processed hero</param>
        private void ExtraProcess(EcsWorld world, ref ActiveEffectComp effect, int subject)
        {
            switch (effect.Effect)
            {
                default: 
                    break;
            }
        }
    }
}
