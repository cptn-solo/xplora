using System.Linq;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAttackSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool = default;
        private readonly EcsPoolInject<DealDamageTag> dealDamageTagPool = default;
        private readonly EcsPoolInject<DealEffectsTag> dealEffectsTagPool = default;
        
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;

        private readonly EcsFilterInject<
            Inc<BattleTurnInfo, MakeTurnTag, AttackTag, AttackerRef, TargetRef>> filter = default;

        private readonly EcsCustomInject<PlayerPreferencesService> prefs = default;
        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                ProcessAttack(entity);
        }

        private void ProcessAttack(int turnEntity)
        {
            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
            ref var targetRef = ref targetRefPool.Value.Get(turnEntity);

            ref var attackerConfig = ref battleService.Value.GetHeroConfig(attackerRef.HeroInstancePackedEntity);
            ref var targetConfig = ref battleService.Value.GetHeroConfig(targetRef.HeroInstancePackedEntity);

            // attack:
            var accurate = prefs.Value.DisableRNGToggle || attackerConfig.RandomAccuracy;

            // defence:
            var dodged = !prefs.Value.DisableRNGToggle && targetConfig.RandomDodge;

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
