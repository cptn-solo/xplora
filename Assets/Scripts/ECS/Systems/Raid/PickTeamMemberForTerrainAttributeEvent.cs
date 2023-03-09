using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;

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
    }
}