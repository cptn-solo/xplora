using System;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAssignAttackerEffectsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<SkippedTag> skippedTagPool = default;
        private readonly EcsPoolInject<AttackerEffectsTag> attackerEffectsTagPool = default;
        private readonly EcsPoolInject<BattleTurnInfo> turnPool = default;
        private readonly EcsPoolInject<EffectsComp> effectsPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;

        private readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                AssignAttackerEffects(entity);
        }

        private void AssignAttackerEffects(int turnEntity)
        {
            ref var turnInfo = ref turnPool.Value.Get(turnEntity);
            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

            if (!attackerRef.HeroInstancePackedEntity.Unpack(out var world, out var attackerInstanceEntity))
                throw new Exception("No Attacker");

            ref var effectsComp = ref effectsPool.Value.Get(attackerInstanceEntity);

            if (effectsComp.ActiveEffects.Count == 0)
                return;

            attackerEffectsTagPool.Value.Add(turnEntity);

            turnInfo.AttackerEffects = effectsComp.ActiveEffects.Keys.ToArray();

            if (effectsComp.SkipTurnActive)
            {
                turnInfo.State = TurnState.TurnSkipped;

                skippedTagPool.Value.Add(turnEntity);
            }

        }
    }
}
