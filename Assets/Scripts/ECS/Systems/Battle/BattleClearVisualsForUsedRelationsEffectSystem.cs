using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearVisualsForUsedRelationsEffectSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;

        private readonly EcsFilterInject<
            Inc<RelationEffectsComp, ProcessedHeroTag>
            > filter = default;

        
        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {

                if (!battleService.Value.RoundEntity.Unpack(out var world, out var roundEntity))
                    throw new Exception("No round");

                ref var roundInfo = ref roundInfoPool.Value.Get(roundEntity);

                ref var relEffects = ref relEffectsPool.Value.Get(entity);
                relEffects.RemoveExpired(roundInfo.Round, out var decrement);

                if (decrement != null)
                {
                    foreach (var item in decrement)
                        if (item.Unpack(out var origWorld, out var origEntity))
                            origWorld.IncrementIntValue<RelationEffectsCountTag>(-1, origEntity);
                }
            }
        }
    }
}
