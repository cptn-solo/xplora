using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class PickTeamMemberForTerrainAttributeEvent :
        PickTeamMemberForEventTraitSystem<TerrainAttributeComp>
    {
        protected override HeroTrait TraitForEvent(int entity)
        {
            ref var attributes = ref pool.Value.Get(entity);
            var eventConfig = worldService.Value
                .TerrainEventsLibrary.TerrainEvents[attributes.Info.TerrainAttribute];

            return eventConfig.Trait;
        }

        protected override bool TryGetTeamMemberForTrait<C>(out int luckyOne, out int level)
            where C : struct
        {
            luckyOne = -1;
            level = 0;
            var filter = ecsWorld.Value.Filter<PlayerTeamTag>().Inc<IntValueComp<C>>().End();
            var pool = ecsWorld.Value.GetPool<IntValueComp<C>>();

            var cnt = filter.GetEntitiesCount();

            if (cnt == 0)
                return false;

            var index = new int[cnt, 2]; // entity + level
            var weight = new float[cnt]; // random weights
            var idx = -1;
            var prev = 0f;

            // filling in weights for trigger hero probe
            foreach (var entity in filter)
            {
                ref var traitValue = ref pool.Get(entity);

                index[++idx, 0] = entity;
                index[idx, 1] = traitValue.Value;

                // probability is in inverse ratio to hero trait level:
                prev = weight[idx] = prev + (1 / traitValue.Value);
            }

            var probe = Random.Range(0, weight[cnt - 1]);

            for (int i = 0; i < cnt; i++)
            {
                if (weight[i] < probe)
                    continue;

                luckyOne = index[i, 0];
                level = index[i, 1];

                // terrain events are randomized by the trigger hero trait level:
                return (5 + (level * 5)).RatedRandomBool();
            }

            return false;

        }
    }
}