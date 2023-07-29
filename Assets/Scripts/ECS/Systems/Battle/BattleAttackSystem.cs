using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAttackSystem : BaseEcsSystem
    {
        private readonly EcsWorldInject ecsWorld = default;
        
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<DealDamageTag> dealDamageTagPool = default;
        private readonly EcsPoolInject<DealEffectsTag> dealEffectsTagPool = default;
        private readonly EcsPoolInject<DodgedTag> dodgedTagPool = default;
        private readonly EcsPoolInject<MissedTag> missedTagPool = default;

        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, AttackerRef, TargetRef>> filter = default;

        private readonly EcsCustomInject<PlayerPreferencesService> prefs = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                ProcessAttack(entity);
        }

        private void ProcessAttack(int turnEntity)
        {
            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
            ref var targetRef = ref targetRefPool.Value.Get(turnEntity);

            if (!attackerRef.Packed.Unpack(out var world, out var attackerEntity))
                throw new Exception("No attacker");

            if (!targetRef.Packed.Unpack(out _, out var targetEntity))
                throw new Exception("No target");

            var accuracyRate = ecsWorld.Value.GetAdjustedIntValue<AccuracyRateTag>(attackerEntity, SpecOption.AccuracyRate);
            var dodgeRate = ecsWorld.Value.GetAdjustedIntValue<DodgeRateTag>(targetEntity, SpecOption.DodgeRate);

            // attack:
            var accurate = prefs.Value.DisableRNGToggle || accuracyRate.RatedRandomBool();
            if (!accurate)
                missedTagPool.Value.Add(attackerEntity);

            // defence:
            var dodged = !prefs.Value.DisableRNGToggle && dodgeRate.RatedRandomBool();
            if (dodged)
                dodgedTagPool.Value.Add(targetEntity);

            if (accurate && !dodged)
            {
                dealDamageTagPool.Value.Add(turnEntity);
                dealEffectsTagPool.Value.Add(turnEntity);
            }            

            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            turnInfo.State = TurnState.TurnInProgress;
        }

    }
}
