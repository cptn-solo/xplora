using System.Linq;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAttackSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<DealDamageTag> dealDamageTagPool = default;
        private readonly EcsPoolInject<DealEffectsTag> dealEffectsTagPool = default;

        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;
        private readonly EcsPoolInject<IntValueComp<AccuracyRateTag>> accuracyPool = default;
        private readonly EcsPoolInject<IntValueComp<DodgeRateTag>> dodgePool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, AttackerRef, TargetRef>> filter = default;

        private readonly EcsCustomInject<PlayerPreferencesService> prefs = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                ProcessAttack(entity);
        }

        private void ProcessAttack(int turnEntity)
        {
            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
            ref var targetRef = ref targetRefPool.Value.Get(turnEntity);

            if (!attackerRef.Packed.Unpack(out _, out var attakerEntity))
                throw new Exception("No attacker");

            if (!targetRef.Packed.Unpack(out _, out var targetEntity))
                throw new Exception("No target");

            ref var accuracyComp = ref accuracyPool.Value.Get(attakerEntity);
            ref var dodgeComp = ref dodgePool.Value.Get(targetEntity);

            // attack:
            var accurate = prefs.Value.DisableRNGToggle || accuracyComp.Value.RatedRandomBool();

            // defence:
            var dodged = !prefs.Value.DisableRNGToggle && dodgeComp.Value.RatedRandomBool();

            if (accurate && !dodged)
            {
                dealDamageTagPool.Value.Add(turnEntity);
                dealEffectsTagPool.Value.Add(turnEntity);
            }

            ref var turnInfo = ref turnInfoPool.Value.Get(turnEntity);
            turnInfo.Dodged = dodged;
            turnInfo.State = TurnState.TurnInProgress;
        }

    }
}
