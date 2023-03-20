﻿using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDealAttackDamageSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;

        private readonly EcsPoolInject<IntValueComp<HpTag>> hpCompPool = default;
        private readonly EcsPoolInject<IntValueComp<HealthTag>> healthCompPool = default;
        private readonly EcsPoolInject<IntValueComp<DefenceRateTag>> defenceCompPool = default;
        private readonly EcsPoolInject<IntRangeValueComp<DamageRangeTag>> damageRangeCompPool = default;
        private readonly EcsPoolInject<IntValueComp<CritRateTag>> critRateCompPool = default;

        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool = default;
        private readonly EcsPoolInject<DealDamageTag> dealDamageTagPool = default;


        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, DealDamageTag>> filter = default;

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;
        private readonly EcsCustomInject<BattleManagementService> battleService = default;
        private readonly EcsCustomInject<PlayerPreferencesService> prefs = default;

        public void Run(IEcsSystems systems)
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

            if (!attackerRef.HeroInstancePackedEntity.Unpack(out var world, out var attackerEntity))
                throw new Exception("No Attacker entity");

            if (!targetRef.HeroInstancePackedEntity.Unpack(out _, out var targetEntity))
                throw new Exception("No Target entity");

            ref var attackerConfig = ref battleService.Value.GetHeroConfig(attackerRef.HeroInstancePackedEntity);

            ref var defenceComp = ref defenceCompPool.Value.Get(targetEntity);
            var shield = defenceComp.Value;
            if (turnInfo.Pierced)
            {
                DamageEffectConfig config = libraryService.Value.DamageTypesLibrary
                    .EffectForDamageType(attackerConfig.DamageType);
                shield = (int)(config.ShieldUseFactor / 100f * shield);
            }

            ref var damageRangeComp = ref damageRangeCompPool.Value.Get(attackerEntity);
            ref var criticalComp = ref critRateCompPool.Value.Get(attackerEntity);

            var rawDamage = prefs.Value.DisableRNGToggle ?
                damageRangeComp.Value.MaxRate : damageRangeComp.RandomValue;
            var criticalDamage = !prefs.Value.DisableRNGToggle && criticalComp.Value.RatedRandomBool();

            int damage = rawDamage;
            damage *= criticalDamage ? 2 : 1;
            damage -= (int)(damage * shield / 100f);
            damage = Mathf.Max(0, damage);

            turnInfo.Damage += damage;
            turnInfo.Critical = criticalDamage;

            ref var hpComp = ref hpCompPool.Value.Get(targetEntity);
            ref var healthComp = ref healthCompPool.Value.Get(targetEntity);

            hpComp.Value = Mathf.Max(0, hpComp.Value - damage);

            ref var barsAndEffectsComp = ref barsAndEffectsPool.Value.Get(targetEntity);
            barsAndEffectsComp.HealthCurrent = hpComp.Value;

        }
    }
}
