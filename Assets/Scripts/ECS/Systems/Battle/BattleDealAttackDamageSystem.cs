using System;
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
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool;
        private readonly EcsPoolInject<TargetRef> targetRefPool;

        private readonly EcsPoolInject<HPComp> hpCompPool;
        private readonly EcsPoolInject<HealthComp> healthCompPool;
        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool;
        private readonly EcsPoolInject<DamageRangeComp> damageRangeCompPool;

        private readonly EcsPoolInject<DealDamageTag> dealDamageTagPool;


        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, DealDamageTag>> filter;

        private readonly EcsCustomInject<HeroLibraryService> libraryService;
        private readonly EcsCustomInject<BattleManagementService> battleService;
        private readonly EcsCustomInject<PlayerPreferencesService> prefs;

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
            ref var targetConfig = ref battleService.Value.GetHeroConfig(targetRef.HeroInstancePackedEntity);

            var shield = targetConfig.DefenceRate;
            if (turnInfo.Pierced)
            {
                DamageEffectConfig config = libraryService.Value.DamageTypesLibrary
                    .EffectForDamageType(attackerConfig.DamageType);
                shield = (int)(config.ShieldUseFactor / 100f * shield);
            }

            ref var damageRangeComp = ref damageRangeCompPool.Value.Get(attackerEntity);

            var rawDamage = prefs.Value.DisableRNGToggle ? damageRangeComp.Max : damageRangeComp.RandomDamage;
            var criticalDamage = !prefs.Value.DisableRNGToggle && attackerConfig.RandomCriticalHit;

            int damage = rawDamage;
            damage *= criticalDamage ? 2 : 1;
            damage -= (int)(damage * shield / 100f);
            damage = Mathf.Max(0, damage);

            turnInfo.Damage += damage;
            turnInfo.Critical = criticalDamage;

            ref var hpComp = ref hpCompPool.Value.Get(targetEntity);
            ref var healthComp = ref healthCompPool.Value.Get(targetEntity);

            hpComp.UpdateHealthCurrent(damage, healthComp.Value, out int aDisplay, out int aCurrent);

            ref var barsAndEffectsComp = ref barsAndEffectsPool.Value.Get(targetEntity);
            barsAndEffectsComp.HealthCurrent = hpComp.Value;

        }
    }
}
