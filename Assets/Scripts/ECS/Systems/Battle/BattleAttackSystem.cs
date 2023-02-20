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
        private readonly EcsPoolInject<BattleTurnInfo> turnInfoPool;
        private readonly EcsPoolInject<AttackTag> attackTagPool;
        private readonly EcsPoolInject<DealDamageTag> dealDamageTagPool;
        private readonly EcsPoolInject<DealEffectsTag> dealEffectsTagPool;
        
        private readonly EcsPoolInject<AttackerRef> attackerRefPool;
        private readonly EcsPoolInject<TargetRef> targetRefPool;

        private readonly EcsFilterInject<Inc<BattleTurnInfo, MakeTurnTag, AttackTag>> filter;

        private readonly EcsCustomInject<PlayerPreferencesService> prefs;
        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ProcessAttack(entity);
                attackTagPool.Value.Del(entity);
            }
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
