using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamUpdateSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<UpdateTag> updateTagPool = default;
        private readonly EcsPoolInject<UpdateTag<HpTag>> updateHPTagPool = default;
        private readonly EcsPoolInject<UpdateTag<BarsInfoComp>> updateBarsInfoPool = default;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, UpdateTag>> filter = default;
        private readonly EcsFilterInject<
            Inc<ToggleTag<ExpandKindsTag>>> panelExpandedToggleFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                updateTagPool.Value.Del(entity);

                updateHPTagPool.Value.Add(entity);
                updateBarsInfoPool.Value.Add(entity);

                var world = systems.GetWorld();
                var expanded = panelExpandedToggleFilter.Value.GetEntitiesCount() > 0;
                world.ToggleEntityButtonByType<CollapseKindsTag>(expanded);
                world.ToggleEntityButtonByType<ExpandKindsTag>(!expanded);
            }
}
    }
}