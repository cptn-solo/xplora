using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.ECS
{
    public static class EcsDamageEffectsExtensions
    {
        public static bool CheckForActiveEffect<T>(this EcsWorld ecsWorld, int entity)
            where T : struct
        {
            var pool = ecsWorld.GetPool<T>();
            return pool.Has(entity);
        }

        public static Dictionary<DamageEffect, int>  GetActiveEffects(this EcsWorld ecsWorld, EcsPackedEntityWithWorld subject)
        {
            var effects = new Dictionary<DamageEffect, int>();
            var pool = ecsWorld.GetPool<ActiveEffectComp>();
            var filter = ecsWorld.Filter<ActiveEffectComp>().Exc<SpecialDamageEffectTag>().End();
            foreach (var entity in filter)
            {
                ref var effect = ref pool.Get(entity);
                
                if (!effect.Subject.EqualsTo(subject))
                    continue;
                
                if (effects.TryGetValue(effect.Effect, out _))
                    effects[effect.Effect] = effect.TurnsActive;
                else
                    effects.Add(effect.Effect, effect.TurnsActive);
            }
            
            return effects;
        }

        public static void UseEffect(this EcsWorld ecsWorld, int entity)
        {
            var pool = ecsWorld.GetPool<ActiveEffectComp>();
            ref var comp = ref pool.Get(entity);
            if (comp.TurnsActive == 0)
            {
                RemoveEffectTag(ecsWorld, comp.Effect, entity, comp.Subject);

                pool.Del(entity);
            }
            else
            {
                comp.TurnsActive--;
            }
        }

        public static ref ActiveEffectComp CastEffect(this EcsWorld ecsWorld, DamageEffect effect, int subject, int turn) =>
            ref CastEffect(ecsWorld, effect, ecsWorld.PackEntityWithWorld(subject), turn);

        public static ref ActiveEffectComp CastEffect(this EcsWorld ecsWorld, DamageEffect effect, EcsPackedEntityWithWorld subject, int turn)
        {
            var entity = ecsWorld.NewEntity();

            ApplyEffectTag(ecsWorld, effect, entity, subject);

            ref var comp = ref ecsWorld.GetPool<ActiveEffectComp>().Add(entity);
            comp.Effect = effect;
            comp.TurnAttached = turn;
            comp.Subject = subject;

            return ref comp;
        }

        #region Add

        private static bool ApplyEffectTag(EcsWorld ecsWorld, DamageEffect effect, int entity, EcsPackedEntityWithWorld subject)
        {
            if (!subject.Unpack(out var world, out var subjEntity))
                throw new Exception("Stale subject for effect attachment");

            return effect switch
            {
                DamageEffect.Stunned => AddToPool<StunnedTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Bleeding => AddToPool<BleedingTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Pierced => AddToPool<PiercedTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Burning => AddToPool<BurningTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Frozing => AddToPool<FrozingTag>(ecsWorld, entity, subjEntity),
                // special cases
                DamageEffect.Critical => AddToPool<CriticalTag, SpecialDamageEffectTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Lethal => AddToPool<LethalTag, SpecialDamageEffectTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Raw => AddToPool<DamageTag, SpecialDamageEffectTag>(ecsWorld, entity, subjEntity),
                _ => false,
            };
        }

        private static bool AddToPool<T>(EcsWorld ecsWorld, int entity, int subjEntity) where T: struct
        {
            var pool = ecsWorld.GetPool<T>();
            
            if (!pool.Has(entity))
                pool.Add(entity);
            
            if (!pool.Has(subjEntity))
                pool.Add(subjEntity);
            
            return true;
        }
        private static bool AddToPool<T, S>(EcsWorld ecsWorld, int entity, int subjEntity) 
            where T : struct
            where S : struct
        {
            _ = AddToPool<S>(ecsWorld, entity, subjEntity);
            _ = AddToPool<T>(ecsWorld, entity, subjEntity);

            return true;
        }

        #endregion

            #region Remove

        private static bool RemoveEffectTag(EcsWorld ecsWorld, DamageEffect effect, int entity, EcsPackedEntityWithWorld subject)
        {
            if (!subject.Unpack(out var world, out var subjEntity))
                throw new Exception("Stale subject for effect attachment");

            return effect switch
            {
                DamageEffect.Stunned => RemoveFromPool<StunnedTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Bleeding => RemoveFromPool<BleedingTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Pierced => RemoveFromPool<PiercedTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Burning => RemoveFromPool<BurningTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Frozing => RemoveFromPool<FrozingTag>(ecsWorld, entity, subjEntity),
                // special cases
                DamageEffect.Critical => RemoveFromPool<CriticalTag, SpecialDamageEffectTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Lethal => RemoveFromPool<LethalTag, SpecialDamageEffectTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Raw => RemoveFromPool<DamageTag, SpecialDamageEffectTag>(ecsWorld, entity, subjEntity),
                _ => false,
            };
        }

        private static bool RemoveFromPool<T>(EcsWorld ecsWorld, int entity, int subjEntity) where T : struct
        {
            var pool = ecsWorld.GetPool<T>();

            if (pool.Has(entity))
                pool.Del(entity);

            if (pool.Has(subjEntity))
                pool.Del(subjEntity);

            return true;
        }
        private static bool RemoveFromPool<T, S>(EcsWorld ecsWorld, int entity, int subjEntity) 
            where T : struct
            where S : struct
        {
            _ = RemoveFromPool<S>(ecsWorld, entity, subjEntity);
            _ = RemoveFromPool<T>(ecsWorld, entity, subjEntity);
            
            return true;
        }

        #endregion
    }
}
