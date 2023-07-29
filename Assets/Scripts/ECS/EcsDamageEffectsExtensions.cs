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
            var filter = ecsWorld.Filter<ActiveEffectComp>().End();
            foreach (var entity in filter)
            {
                ref var effect = ref pool.Get(entity);
                
                if (!effect.Subject.EqualsTo(subject))
                    continue;
                
                if (effect.Effect switch
                {
                    DamageEffect.Critical => false,
                    DamageEffect.Lethal => false,
                    _ => true
                })
                    continue; // special cases

                if (effects.TryGetValue(effect.Effect, out _))
                    effects[effect.Effect]++;
                else
                    effects.Add(effect.Effect, 1);
            }
            
            return effects;
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
                DamageEffect.Critical => AddToPool<CriticalTag>(ecsWorld, entity, subjEntity),
                DamageEffect.Lethal => AddToPool<LethalTag>(ecsWorld, entity, subjEntity),
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

    }
}
