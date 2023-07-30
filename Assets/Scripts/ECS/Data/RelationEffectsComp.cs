using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ECS.Data
{
    public struct RelationEffectsComp
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

        public void RemoveExpired(
            out EcsPackedEntityWithWorld[] ptpToDecrement)
        {
            ptpToDecrement = null;
            
            if (CurrentEffects == null) return;

            var buffer = ListPool<RelationEffectKey>.Get();
            var removed = ListPool<EcsPackedEntityWithWorld>.Get();

            foreach (var item in CurrentEffects)
            {
                if (!item.Value.Unpack(out var world, out var entity))
                    throw new Exception("Stale rel effect instance");

                var pool = world.GetPool<EffectInstanceInfo>();
                if (!pool.Has(entity))
                    throw new Exception("No rel effect instance for entity");

                ref var effect = ref pool.Get(entity);
                if (effect.UsageLeft > 0)
                    continue;

                buffer.Add(item.Key);
                removed.Add(effect.EffectP2PEntity);
                
                world.DelEntity(entity);
            }

            foreach (var item in buffer)
                CurrentEffects.Remove(item);
            
            ptpToDecrement = removed.ToArray();

            ListPool<RelationEffectKey>.Add(buffer);
            ListPool<EcsPackedEntityWithWorld>.Add(removed);

            CurrentEffectsInfo = GetCurrentEffectsInfo();
        }

        internal void RemoveByType(RelationsEffectType relationsEffectType, 
            out EcsPackedEntityWithWorld[] ptpToDecrement)
        {
            ptpToDecrement = null;

            if (CurrentEffects == null) return;

            var buffer = ListPool<RelationEffectKey>.Get();
            var removed = ListPool<EcsPackedEntityWithWorld>.Get();


            foreach (var item in CurrentEffects)
            {
                if (!item.Value.Unpack(out var world, out var entity))
                    throw new Exception("Stale rel effect instance");

                var pool = world.GetPool<EffectInstanceInfo>();
                if (!pool.Has(entity))
                    throw new Exception("No rel effect instance for entity");

                ref var effect = ref pool.Get(entity);
                if (effect.Rule.EffectType != relationsEffectType)
                    continue;

                buffer.Add(item.Key);
                removed.Add(effect.EffectP2PEntity);

                world.DelEntity(entity);
            }

            foreach (var item in buffer)
                CurrentEffects.Remove(item);
            
            ptpToDecrement = removed.ToArray();

            ListPool<RelationEffectKey>.Add(buffer);            
            ListPool<EcsPackedEntityWithWorld>.Add(removed);

            CurrentEffectsInfo = GetCurrentEffectsInfo();
        }


        public void SetEffect(RelationEffectKey type, EcsPackedEntityWithWorld effect)
        {
            if (CurrentEffects == null) return;
            
            if (CurrentEffects.Count > MaxEffectsForHero) return;

            if (CurrentEffects.TryGetValue(type, out _))
            {
                CurrentEffects[type] = effect;
            }
            else
            {
                CurrentEffects.Add(type, effect);
            }

            CurrentEffectsInfo = GetCurrentEffectsInfo();
        }
    }
}
