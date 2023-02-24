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
        private readonly EcsPoolInject<HPComp> hpCompPool;
        private readonly EcsPoolInject<HealthComp> healthCompPool;
        private readonly EcsPoolInject<PositionComp> positionPool;
        private readonly EcsPoolInject<EffectsComp> effectsPool;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool;
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool;

        private readonly EcsPoolInject<AttackerEffectsTag> attackerEffectsTagPool;
        private readonly EcsPoolInject<AttackTag> attackTagPool;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, MakeTurnTag, AttackerEffectsTag>> filter;

        private readonly EcsCustomInject<HeroLibraryService> libraryService;
        private readonly EcsCustomInject<BattleManagementService> battleService;

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
            ref var position = ref positionPool.Value.Get(attackerEntity);

            hpComp.UpdateHealthCurrent(effectDamage, healthComp.Value, out int aDisplay, out int aCurrent);

            // intermediate turn info, no round turn override to preserve pre-calculated target:
            var effectsInfo = new BattleTurnInfo() {
                Turn = turnInfo.Turn,
                Attacker = turnInfo.Attacker,
                AttackerPosition = position.Position,
                Damage = effectDamage,
                AttackerEffects = effs,
                Lethal = hpComp.Value <= 0,
                //Health = healthComp.Value,
                //HealthCurrent = hpComp.HP,
                //Speed = turnInfo.Attacker.Speed, // this should be taken from somewhere else
                //ActiveEffects = effectsComp.ActiveEffects,
                State = TurnState.TurnEffects,
            };

            battleService.Value.NotifyTurnEventListeners(effectsInfo);

            if (hpComp.Value > 0)
                attackTagPool.Value.Add(turnEntity);
        }


    }
}
