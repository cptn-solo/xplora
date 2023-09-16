using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.World;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using static UnityEngine.EventSystems.EventTrigger;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamUpdateScoreSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EntityViewRef<TeamMemberInfo>> entityViewRefPool = default;
     
        private readonly EcsFilterInject<
            Inc<EntityViewRef<TeamMemberInfo>,
                HoverTag>> hoverFilter = default;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<TeamMemberInfo>>,
            Exc<HoverTag>> nothoveredFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            if (hoverFilter.Value.GetEntitiesCount() > 0)
            {
                foreach (var entity in hoverFilter.Value)
                {
                    ref var entityViewRef = ref entityViewRefPool.Value.Get(entity);
                    var card = (TeamMember)entityViewRef.EntityView;

                    card.SetScore(null);

                    foreach (var nothoveredEntity in nothoveredFilter.Value)
                    {
                        ref var notHoveredViewRef = ref entityViewRefPool.Value.Get(nothoveredEntity);
                        var notHoveredCard = (TeamMember)notHoveredViewRef.EntityView;
                        var score = world.GetRelationScore(entity, nothoveredEntity);

                        notHoveredCard.SetScore(score);
                    }
                }
            }
            else
            {
                foreach (var nothoveredEntity in nothoveredFilter.Value)
                {
                    ref var notHoveredViewRef = ref entityViewRefPool.Value.Get(nothoveredEntity);
                    var notHoveredCard = (TeamMember)notHoveredViewRef.EntityView;

                    notHoveredCard.SetScore(null);
                }
            }
        }
    }
}