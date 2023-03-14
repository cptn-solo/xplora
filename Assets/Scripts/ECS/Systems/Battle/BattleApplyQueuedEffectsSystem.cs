using System;
using System.Linq;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleApplyQueuedEffectsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<HPComp> hpCompPool = default;
        private readonly EcsPoolInject<HealthComp> healthCompPool = default;
        private readonly EcsPoolInject<EffectsComp> effectsPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;

        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool = default;
        private readonly EcsPoolInject<AttackerEffectsTag> attackerEffectsTagPool = default;
        private readonly EcsPoolInject<AttackTag> attackTagPool = default;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, MakeTurnTag, AttackerEffectsTag>> filter = default;

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;
        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ApplyQueuedEffects(entity);
                attackerEffectsTagPool.Value.Del(entity);
            }
        }
        //private void ApplyQueuedEffects(BattleTurnInfo turnInfo, out Hero attacker, out BattleTurnInfo? effectsInfo)
        private void ApplyQueuedEffects(int turnEntity)
        {
            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

            if (!attackerRef.HeroInstancePackedEntity.Unpack(out var world, out var attackerEntity))
                throw new Exception("No Attacker");

            ref var effectsComp = ref effectsPool.Value.Get(attackerEntity);
            var effs = effectsComp.ActiveEffects.Keys.ToArray(); // will be used to flash used effects ===>

            var effectDamage = 0;
            foreach (var eff in effs)
            {
                effectDamage += libraryService.Value.DamageTypesLibrary
                    .EffectForDamageEffect(eff).ExtraDamage;
                effectsComp.UseEffect(eff, out var used);
            }

            ref var hpComp = ref hpCompPool.Value.Get(attackerEntity);
            ref var healthComp = ref healthCompPool.Value.Get(attackerEntity);

            hpComp.UpdateHealthCurrent(effectDamage, healthComp.Value, out int aDisplay, out int aCurrent);

            ref var barsAndEffectsComp = ref barsAndEffectsPool.Value.Get(attackerEntity);
            barsAndEffectsComp.ActiveEffects = effectsComp.ActiveEffects;
            barsAndEffectsComp.HealthCurrent = hpComp.Value;

            // intermediate turn info, no round turn override to preserve pre-calculated target:
            var effectsInfo = new BattleTurnInfo() {
                Turn = turnInfo.Turn,
                Attacker = turnInfo.Attacker,
                Damage = effectDamage,
                AttackerEffects = effs,
                Lethal = hpComp.Value <= 0,
                State = TurnState.TurnEffects,
            };

            battleService.Value.NotifyTurnEventListeners(effectsInfo);

            if (hpComp.Value <= 0)
                attackTagPool.Value.Del(turnEntity);
        }


    }
}
