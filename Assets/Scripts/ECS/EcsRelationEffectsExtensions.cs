using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    public static class EcsRelationEffectsExtensions
    {
        #region Helpers and filters

        public static ref HeroInstanceMapping GetHeroInstanceMappings(this EcsWorld world)
        {
            var filter = world.Filter<HeroInstanceMapping>().End();
            var pool = world.GetPool<HeroInstanceMapping>();
            foreach (var entity in filter)
                return ref pool.Get(entity);

            throw new Exception("Mappings not defined");
        }

        public static ref RelationsMatrixComp GetRelationsMatrix(this EcsWorld origWorld)
        {
            var matrixFilter = origWorld.Filter<RelationsMatrixComp>().End();
            var matrixPool = origWorld.GetPool<RelationsMatrixComp>();

            foreach (var matrixEntity in matrixFilter)
            {
                ref var matrixComp = ref matrixPool.Get(matrixEntity);
                return ref matrixComp;
            }

            throw new Exception("No relations matrix defined for the world");
        }
        public static IEnumerator<EffectInstanceInfo> SubjectEffects(this EcsWorld world, int subjectEntity)
        {
            var effectInstancePool = world.GetPool<EffectInstanceInfo>();
            var en = SubjectEffectsEntities(world, subjectEntity);
            while (en.MoveNext())
                yield return effectInstancePool.Get(en.Current);
        }

        public static IEnumerator<int> SubjectEffectsOfTypeEntities(this EcsWorld world, int subjectEntity, RelationsEffectType relationsEffectType)
        {
            var effectInstancePool = world.GetPool<EffectInstanceInfo>();
            var en = SubjectEffectsEntities(world, subjectEntity);
            while (en.MoveNext())
            {
                var effect = effectInstancePool.Get(en.Current);
                if (effect.Rule.Key.RelationsEffectType == relationsEffectType)
                    yield return en.Current;
            }
        }

        public static IEnumerator<int> SubjectEffectsOfFullKeyEntities(this EcsWorld world, int subjectEntity, RelationEffectKey key)
        {
            var effectInstancePool = world.GetPool<EffectInstanceInfo>();
            var en = SubjectEffectsEntities(world, subjectEntity);
            while (en.MoveNext())
            {
                var effect = effectInstancePool.Get(en.Current);
                if (effect.Rule.Key.Equals(key))
                    yield return en.Current;
            }
        }

        public static IEnumerator<int> SubjectEffectsEntities(this EcsWorld world, int subjectEntity)
        {
            var effectsFilter = world.Filter<EffectInstanceInfo>().End();
            var effectInstancePool = world.GetPool<EffectInstanceInfo>();

            var mappings = world.GetHeroInstanceMappings();
            var subjectOrigPacked = mappings.BattleToOriginMapping[world.PackEntityWithWorld(subjectEntity)];

            foreach (var effectEntity in effectsFilter)
            {
                var relEffect = effectInstancePool.Get(effectEntity);

                if (!relEffect.EffectTarget.EqualsTo(subjectOrigPacked))
                    continue;

                yield return effectEntity;
            }
        }

        #endregion

        #region Utilities

        public static bool TrySpawnAdditionalEffect(this EcsWorld origWorld, int p2pEntity, HeroRelationEffectsLibrary effectRules)
        {
            var currentEffectsCount = origWorld.ReadIntValue<RelationEffectsCountTag>(p2pEntity);
            // respect spawn rate from AdditionalEffectSpawnRate:
            return effectRules.TrySpawnAdditionalEffect(currentEffectsCount);
        }

        public static bool GetEffectConfigForProbe(this EcsWorld origWorld, int p2pEntity, 
            HeroRelationEffectsLibrary effectRules, HeroRelationsConfig relationsConfig,
            RelEffectProbeComp probe, 
            out HeroRelationEffectConfig scope
            )
        {
            var score = origWorld.ReadIntValue<RelationScoreTag>(p2pEntity);
            var relationsState = relationsConfig.GetRelationState(score);

            if (!probe.TargetConfigRefPacked.Unpack(out var libWorld, out var tgtHeroConfigEntity))
                throw new Exception("No Hero Config for current (recepient) guy");

            if (!probe.SourceConfigRefPacked.Unpack(out _, out var srcHeroConfigEntity))
                throw new Exception("No Hero Config for other (spawner) guy");

            var libHeroPool = libWorld.GetPool<Hero>();
            ref var tgtHeroConfig = ref libHeroPool.Get(tgtHeroConfigEntity);
            ref var srcHeroConfig = ref libHeroPool.Get(srcHeroConfigEntity);

            var rulesCaseKey = new RelationEffectLibraryKey(
                srcHeroConfig.Id, probe.SubjectState, relationsState);

            if (!effectRules.SubjectStateEffectsIndex.TryGetValue(rulesCaseKey, out scope))
                return false; // no effect for relation state, it's ok

            Debug.Log($"Relations Effect of type {scope.EffectRule.EffectType} was just spawned " +
                $"for {tgtHeroConfig.Name} in {scope.SelfState} due to {scope.RelationState} " +
                $"with {srcHeroConfig.Name}");

            return true;
        }


        public static float GetResistanceFactor(this EcsWorld world, int subjectEntity, DamageEffect eff)
        {
            var retval = 1f;
            
            var en = SubjectEffects(world, subjectEntity);
            
            while (en.MoveNext())
            {
                var relEffect = en.Current;

                switch (relEffect.Rule.EffectType)
                {
                    case RelationsEffectType.SpecAbs:
                        {
                            var rule = (EffectRuleSpecAbs)relEffect.Rule;
                            if (rule.SpecOption == eff.ResistanceSpec())
                                retval -= rule.Value / 100f;
                        }
                        break;
                    case RelationsEffectType.SpecPercent:
                        {
                            var rule = (EffectRuleSpecAbs)relEffect.Rule;
                            if (rule.SpecOption == eff.ResistanceSpec())
                                retval *= rule.Value / 100f;
                        }
                        break;
                    default:
                        break;
                }
            }

            return retval;
        }

        public static void UseRelationEffect(this EcsWorld world, int effectEntity)
        {
            var effectInstancePool = world.GetPool<EffectInstanceInfo>();
            var relEffect = effectInstancePool.Get(effectEntity);
            relEffect.UsageLeft--;
        }

        public static void RemoveRelEffectByType(this EcsWorld world, int subjectEntity, RelationsEffectType relationsEffectType,
            out EcsPackedEntityWithWorld[] ptpToDecrement)
        {
            var removed = ListPool<EcsPackedEntityWithWorld>.Get();
            
            var en = SubjectEffectsOfTypeEntities(world, subjectEntity, relationsEffectType);
            var effectInstancePool = world.GetPool<EffectInstanceInfo>();

            while (en.MoveNext())
            {
                var relEffect = effectInstancePool.Get(en.Current);

                removed.Add(relEffect.EffectP2PEntity);

                world.DelEntity(en.Current);
            }

            ptpToDecrement = removed.ToArray();

            ListPool<EcsPackedEntityWithWorld>.Add(removed);
        }

        public static void RemoveExpiredRelEffects(this EcsWorld world, int subjectEntity,
            out EcsPackedEntityWithWorld[] ptpToDecrement)
        {

            var removed = ListPool<EcsPackedEntityWithWorld>.Get();

            var en = SubjectEffectsEntities(world, subjectEntity);
            var effectInstancePool = world.GetPool<EffectInstanceInfo>();

            while (en.MoveNext())
            {
                var relEffect = effectInstancePool.Get(en.Current);

                if (relEffect.UsageLeft > 0)
                    continue;

                removed.Add(relEffect.EffectP2PEntity);

                world.DelEntity(en.Current);
            }

            ptpToDecrement = removed.ToArray();

            ListPool<EcsPackedEntityWithWorld>.Add(removed);
        }

        #endregion

        // ### REFACTORING PENDING:
        public static bool GetAdjustedBoolValue<T>(this EcsWorld ecsWorld, int entity, SpecOption specOption)
            where T : struct
        {
            var adjValue = ecsWorld.GetAdjustedIntValue<T>(entity, specOption);
            return adjValue.RatedRandomBool();
        }

        public static int GetAdjustedIntValue<T>(this EcsWorld ecsWorld, int entity, SpecOption specOption)
            where T : struct
        {
            var rawValue = ecsWorld.ReadIntValue<T>(entity);
            var adjType = ecsWorld.GetRelationAdjustment(entity, specOption, out var factor, out var value);
            var adjValue = adjType switch
            {
                AdjustmentType.Factor => (int)(rawValue * factor),
                AdjustmentType.Value => value,
                _ => rawValue,
            };
            return adjValue;
        }

        public static int GetAdjustedRangedValue<T>(this EcsWorld ecsWorld, int entity, SpecOption specOption)
            where T : struct
        {
            var rawRange = ecsWorld.ReadIntRangeValue<T>(entity);
            var rawValue = rawRange.RandomValue;
            var adjType = ecsWorld.GetRelationAdjustment(entity, specOption, out var factor, out var value, rawRange);

            return adjType switch
            {
                AdjustmentType.Factor => (int)(rawValue * factor),
                AdjustmentType.Value => value,
                _ => rawValue,
            };
        }
        
        public static bool GetDamageTypeBlock(this EcsWorld ecsWorld, int entity, DamageType damageType)
        {
            var key = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, damageType, RelationsEffectType.AlgoDamageTypeBlock);
            var adjType = ecsWorld.GetRelationAdjustment(entity, key, out _, out var value);
            var adjValue = adjType switch
            {
                AdjustmentType.Value => value == 1,
                _ => false,
            };
            return adjValue;
        }

        public static bool GetAlgoRevengeFocus(this EcsWorld ecsWorld, int entity, out EcsPackedEntityWithWorld? focus, out int focusEntity) =>
            ecsWorld.GetRelEffectFocus(entity, RelationsEffectType.AlgoRevenge, out focus, out focusEntity);

        public static bool GetAlgoTargetFocus(this EcsWorld ecsWorld, int entity, out EcsPackedEntityWithWorld? focus, out int focusEntity) =>
            ecsWorld.GetRelEffectFocus(entity, RelationsEffectType.AlgoTarget, out focus, out focusEntity);

        public static bool GetRelEffectFocus(this EcsWorld ecsWorld, int actorProbe, RelationsEffectType effectType, 
            out EcsPackedEntityWithWorld? focus,
            out int focusEntity)
        {
            focus = null;
            focusEntity = -1;
            var filter = ecsWorld.Filter<EffectFocusComp>().End();
            foreach (var entity in filter)
            {
                ref var focusComp = ref ecsWorld.GetPool<EffectFocusComp>().Get(entity);

                if (!focusComp.Actor.Unpack(out var world, out var actorEntity))
                    throw new Exception("Stale Actor");

                if (!ecsWorld.Equals(world))
                    continue;

                if (focusComp.EffectKey.RelationsEffectType != effectType)
                    continue;

                if (actorEntity == actorProbe)
                {
                    focus = focusComp.Focused;
                    focusEntity = entity;
                    return true;
                }    
            }
            return false;
        }


        public static AdjustmentType GetDamageEffect(this EcsWorld ecsWorld, int entity,
            DamageEffect damageEffect, out float factor, out int value, IntRange rangeValue = null)
        {
            var key = new RelationEffectKey(SpecOption.NA, damageEffect, DamageType.NA, RelationsEffectType.DmgEffectKey);
            return ecsWorld.GetRelationAdjustment(entity, key, out factor, out value, rangeValue);
        }

        public static AdjustmentType GetDamageEffectBonus(this EcsWorld ecsWorld, int entity,
            out float factor, out int value, IntRange rangeValue = null)
        {
            var key = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, DamageType.NA, RelationsEffectType.DmgEffectBonusKey);
            return ecsWorld.GetRelationAdjustment(entity, key, out factor, out value, rangeValue);
        }

        public static AdjustmentType GetRelationAdjustment(this EcsWorld ecsWorld, int entity,
            SpecOption specOption, out float factor, out int value, IntRange rangeValue = null)
        {
            var key = new RelationEffectKey(specOption, DamageEffect.NA, DamageType.NA, RelationsEffectType.SpecKey);
            return ecsWorld.GetRelationAdjustment(entity, key, out factor, out value, rangeValue);
        }

        public static AdjustmentType GetRelationAdjustment(this EcsWorld world, int subjectEntity,
            RelationEffectKey key, out float factor, out int value, IntRange rangeValue = null)
        {
            value = 0;
            factor = 1f;

            var en = SubjectEffects(world, subjectEntity);

            while (en.MoveNext())
            {
                var relEffect = en.Current;

                switch (key.RelationsEffectType)
                {
                    case RelationsEffectType.SpecKey:
                        if (key.SpecOption != SpecOption.NA && relEffect.Rule.Key.SpecOption == key.SpecOption)
                            return GetSpecKeyAdjustment(relEffect, out factor, out value, rangeValue);
                        break;
                    case RelationsEffectType.DmgEffectKey:
                        if (key.DamageEffect != DamageEffect.NA && relEffect.Rule.Key.DamageEffect == key.DamageEffect)
                            return GetDmgEffectKeyAdjustment(relEffect, out factor, out value);
                        break;
                    case RelationsEffectType.DmgEffectBonusKey:
                        if (relEffect.Rule.Key.RelationsEffectType switch
                        {
                            RelationsEffectType.DmgEffectBonusAbs => true,
                            RelationsEffectType.DmgEffectBonusPercent => true,
                            _ => false
                        }) return GetDmgEffectBonusKeyAdjustment(relEffect, out factor, out value);
                        break;
                    case RelationsEffectType.AlgoDamageTypeBlock:
                        if (key.DamageType != DamageType.NA && relEffect.Rule.Key.DamageType == key.DamageType)
                            return GetAlgoDamageTypeBlockAdjustment(relEffect, out value);
                        break;
                    case RelationsEffectType.AlgoRevenge:
                        //TODO:
                        break;
                    case RelationsEffectType.AlgoTarget:
                        //TODO:
                        break;
                    default:
                        //actually invalid entry as we are always know the key type exactly
                        break;
                }
            }

            return AdjustmentType.NA;
        }

        private static AdjustmentType GetAlgoDamageTypeBlockAdjustment(EffectInstanceInfo relEffect, out int value)
        {
            value = 0;
            switch (relEffect.Rule.EffectType)
            {
                case RelationsEffectType.AlgoDamageTypeBlock:
                    {
                        var rule = (EffectRuleAlgoDamageTypeBlock)relEffect.Rule;
                        value = rule.Flag;
                        return AdjustmentType.Value;
                    }
                default:
                    return AdjustmentType.NA;
            }
        }

        private static AdjustmentType GetDmgEffectBonusKeyAdjustment(EffectInstanceInfo relEffect, out float factor, out int value)
        {
            value = 0;
            factor = 1f;
            switch (relEffect.Rule.EffectType)
            {
                case RelationsEffectType.DmgEffectBonusAbs:
                    {
                        var rule = (EffectRuleDmgEffectBonusAbs)relEffect.Rule;
                        factor += rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                case RelationsEffectType.DmgEffectBonusPercent:
                    {
                        var rule = (EffectRuleDmgEffectBonusPercent)relEffect.Rule;
                        factor *= rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                default:
                    return AdjustmentType.NA;
            }
        }

        private static AdjustmentType GetDmgEffectKeyAdjustment(EffectInstanceInfo relEffect, out float factor, out int value)
        {
            value = 0;
            factor = 1f;
            switch (relEffect.Rule.EffectType)
            {
                case RelationsEffectType.DmgEffectAbs:
                    {
                        var rule = (EffectRuleDmgEffectAbs)relEffect.Rule;
                        factor += rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                case RelationsEffectType.DmgEffectPercent:
                    {
                        var rule = (EffectRuleDmgEffectPercent)relEffect.Rule;
                        factor *= rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                default:
                    return AdjustmentType.NA;
            }
        }

        private static AdjustmentType GetSpecKeyAdjustment(EffectInstanceInfo relEffect, out float factor, out int value, IntRange rangeValue = null)
        {
            value = 0;
            factor = 1f;
            switch (relEffect.Rule.EffectType)
            {
                case RelationsEffectType.SpecMaxMin:
                    {
                        if (rangeValue == null)
                            throw new Exception("RelationsEffectType.SpecMaxMin requires ranged value");

                        var rule = (EffectRuleSpecMaxMin)relEffect.Rule;
                        value = rule.MaxMin > 0 ? rangeValue.MaxRate : rangeValue.MinRate;
                        return AdjustmentType.Value;
                    }
                case RelationsEffectType.SpecAbs:
                    {
                        var rule = (EffectRuleSpecAbs)relEffect.Rule;
                        factor += rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                case RelationsEffectType.SpecPercent:
                    {
                        var rule = (EffectRuleSpecPercent)relEffect.Rule;
                        factor *= rule.Value / 100f;
                        return AdjustmentType.Factor;
                    }
                default:
                    return AdjustmentType.NA;
            }
        }
    }
}
