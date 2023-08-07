using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ECS.Data
{
    public struct RelationEffectsComp_
    {        
        private const int MaxEffectsForHero = 99; // decision made to let them spawn as they are


        public Dictionary<RelationEffectKey, EcsPackedEntityWithWorld> CurrentEffects;

        public RelationEffectInfo[] CurrentEffectsInfo { get; private set; }

        private readonly RelationEffectInfo[] GetCurrentEffectsInfo() { 
            var buffer = ListPool<RelationEffectInfo>.Get();

            foreach (var item in CurrentEffects)
            {
                if (!item.Value.Unpack(out var world, out var entity))
                    throw new Exception("Stale rel effect instance");

                var pool = world.GetPool<EffectInstanceInfo>();
                if (!pool.Has(entity))
                    throw new Exception("No rel effect instance for entity");

                ref var effect = ref pool.Get(entity);
                if (effect.UsageLeft <= 0)
                    continue;

                buffer.Add(effect.EffectInfo);
            }

            var retval = buffer.ToArray();
            ListPool<RelationEffectInfo>.Add(buffer);

            return retval;
        }

        public void Clear(
            out EcsPackedEntityWithWorld[] ptpToDecrement)
        {
            ptpToDecrement = null;
            var removed = ListPool<EcsPackedEntityWithWorld>.Get();

            foreach (var item in CurrentEffects)
            {
                if (!item.Value.Unpack(out var world, out var entity))
                    throw new Exception("Stale rel effect instance");

                var pool = world.GetPool<EffectInstanceInfo>();
                if (!pool.Has(entity))
                    throw new Exception("No rel effect instance for entity");

                ref var effect = ref pool.Get(entity);
                removed.Add(effect.EffectP2PEntity);

                world.DelEntity(entity);
            }

            ptpToDecrement = removed.ToArray();

            ListPool<EcsPackedEntityWithWorld>.Add(removed);

            CurrentEffects.Clear();

            CurrentEffectsInfo = GetCurrentEffectsInfo();
        }
        
    }
}
