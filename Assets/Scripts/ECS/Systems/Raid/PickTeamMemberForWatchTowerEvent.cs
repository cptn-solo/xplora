using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;

namespace Assets.Scripts.ECS.Systems
{
    public class PickTeamMemberForWatchTowerEvent :
        PickTeamMemberForEventTraitSystem<WatchTowerComp>
    {
        protected override HeroTrait TraitForEvent(int entity)
        {
            return HeroTrait.Scout;
        }
    }
}