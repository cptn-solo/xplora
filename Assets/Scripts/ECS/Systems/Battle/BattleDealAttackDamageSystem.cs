using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDealAttackDamageSystem : BaseEcsSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;
        private readonly EcsPoolInject<DealDamageTag> dealDamageTagPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, DealDamageTag>> filter = default;

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;
        private readonly EcsCustomInject<BattleManagementService> battleService = default;
        private readonly EcsCustomInject<PlayerPreferencesService> prefs = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                DealDamage(entity);
                dealDamageTagPool.Value.Del(entity);
            }
        }

        private void DealDamage(int turnEntity)
        {
            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);

            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
            ref var targetRef = ref targetRefPool.Value.Get(turnEntity);

            if (!attackerRef.HeroInstancePackedEntity.Unpack(out _, out var attackerEntity))
                throw new Exception("No Attacker entity");

            if (!targetRef.HeroInstancePackedEntity.Unpack(out _, out var targetEntity))
                throw new Exception("No Target entity");

            ref var attackerConfig = ref battleService.Value.GetHeroConfig(attackerRef.HeroInstancePackedEntity);

            var damageBlockedByRelations = ecsWorld.Value.GetDamageTypeBlock(targetEntity, attackerConfig.DamageType);
            if (damageBlockedByRelations)
            {
                Debug.Log($"Damage Blocked for {attackerConfig.DamageType} By Relations Effect!");
                //TODO: communicate to UI/in-game logs
                return;
            }

            int shield = ecsWorld.Value.GetAdjustedIntValue<DefenceRateTag>(targetEntity, SpecOption.DefenceRate);
            
            if (ecsWorld.Value.CheckForActiveEffect<PiercedTag>(targetEntity))
            {
                DamageEffectConfig config = libraryService.Value.DamageTypesLibrary
                    .ConfigForDamageType(attackerConfig.DamageType);
                shield = (int)(config.ShieldUseFactor / 100f * shield);
            }

            int rawDamage;
            if (prefs.Value.DisableRNGToggle)
            {
                var damageRange = ecsWorld.Value.ReadIntRangeValue<DamageRangeTag>(attackerEntity);
                rawDamage = damageRange.MaxRate;
            }
            else
            {
                rawDamage = ecsWorld.Value.GetAdjustedRangedValue<DamageRangeTag>(attackerEntity, SpecOption.DamageRange);
            }

            bool criticalDamage;
            if (prefs.Value.DisableRNGToggle)
            {
                criticalDamage = true;
            }
            else
            {
                criticalDamage = ecsWorld.Value.GetAdjustedBoolValue<CritRateTag>(attackerEntity, SpecOption.CritRate);
            }            

            int damage = rawDamage;
            damage *= criticalDamage ? 2 : 1;
            damage -= (int)(damage * shield / 100f);
            damage = Mathf.Max(1, damage); // no reason to deal 0 damage bc
                                           // this is only possible due to
                                           // roundings. changing 0 to 1

            ecsWorld.Value.IncrementIntValue<DamageTag>(damage, targetEntity);
            
            if (criticalDamage)
                ecsWorld.Value.CastEffect(DamageEffect.Critical, targetEntity, turnInfo.Turn);            
        }
    }
}
