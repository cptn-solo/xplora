using System;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleTryCastEffectsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<DealEffectsTag> dealEffectsTagPool;

        private readonly EcsPoolInject<AttackerRef> attackerRefPool;
        private readonly EcsPoolInject<TargetRef> targetRefPool;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool;

        private readonly EcsPoolInject<HPComp> hpCompPool;
        private readonly EcsPoolInject<HealthComp> healthCompPool;
        private readonly EcsPoolInject<EffectsComp> effectsPool;
        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, DealEffectsTag>> filter;

        private readonly EcsCustomInject<PlayerPreferencesService> prefs;
        private readonly EcsCustomInject<HeroLibraryService> libraryService;
        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                DealEffectsDamage(entity);
                dealEffectsTagPool.Value.Del(entity);
            }
        }

        private void DealEffectsDamage(int turnEntity)
        {

            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
            ref var targetRef = ref targetRefPool.Value.Get(turnEntity);

            if (!targetRef.HeroInstancePackedEntity.Unpack(out var world, out var targetEntity))
                throw new Exception("No target");

            ref var attackerConfig = ref battleService.Value.GetHeroConfig(attackerRef.HeroInstancePackedEntity);
            ref var targetConfig = ref battleService.Value.GetHeroConfig(targetRef.HeroInstancePackedEntity);

            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            ref var currentRound = ref battleService.Value.CurrentRound;

            if (TryCast(attackerConfig, targetConfig, currentRound.Round, out var damageEffect,
                prefs.Value.DisableRNGToggle))
            {
                if (damageEffect.Config.Effect == DamageEffect.Pierced)
                {
                    turnInfo.Pierced = true;
                }
                else
                {
                    turnInfo.TargetEffects = turnInfo.TargetEffects.Concat(
                        new DamageEffect[]{ damageEffect.Config.Effect }).ToArray();
                    turnInfo.ExtraDamage = damageEffect.Config.ExtraDamage;
                    turnInfo.Damage += turnInfo.ExtraDamage;

                    ref var effectsComp = ref effectsPool.Value.Get(targetEntity);
                    effectsComp.EnqueEffect(damageEffect);

                    ref var hpComp = ref hpCompPool.Value.Get(targetEntity);
                    ref var healthComp = ref healthCompPool.Value.Get(targetEntity);

                    hpComp.UpdateHealthCurrent(turnInfo.ExtraDamage, healthComp.Value, out int aDisplay, out int aCurrent);

                    ref var barsAndEffectsComp = ref barsAndEffectsPool.Value.Get(targetEntity);
                    barsAndEffectsComp.ActiveEffects = effectsComp.ActiveEffects;
                    barsAndEffectsComp.HealthCurrent = hpComp.Value;
                }
            }
        }

        private bool TryCast(Hero attacker, Hero target, int roundOn, out DamageEffectInfo info,
            bool disableRNGToggle)
        {
            DamageEffectConfig config = libraryService.Value.DamageTypesLibrary
                .EffectForDamageType(attacker.DamageType);

            info = default;

            if (config.Effect == DamageEffect.NA)
                return false;

            if (!disableRNGToggle)
            {

                if (!config.ChanceRate.RatedRandomBool()) return false;

                switch (config.Effect)
                {
                    case DamageEffect.Stunned:
                        if (target.RandomResistStun) return false;
                        break;
                    case DamageEffect.Bleeding:
                        if (target.RandomResistBleeding) return false;
                        break;
                    case DamageEffect.Pierced:
                        if (target.RandomResistPierced) return false;
                        break;
                    case DamageEffect.Burning:
                        if (target.RandomResistBurning) return false;
                        break;
                    case DamageEffect.Frozing:
                        if (target.RandomResistFrozing) return false;
                        break;
                    default:
                        return false;
                }
            }
            DamageEffectInfo draft = DamageEffectInfo.Draft(config);

            draft = draft.SetDuration(roundOn, roundOn + config.TurnsActive);

            info = draft;

            return true;
        }

    }
}
