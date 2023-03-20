using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class PickTeamMemberForWatchTowerEvent :
        PickTeamMemberForEventTraitSystem<WatchTowerComp>
    {
        protected override HeroTrait TraitForEvent(int entity)
        {
            return HeroTrait.Scout;
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

            foreach (var entity in filter)
            {
                ref var traitValue = ref pool.Get(entity);

                if (traitValue.Value <= level)
                    continue;

                level = traitValue.Value;
                luckyOne = entity;
            }

            return true;
        }
    }
}