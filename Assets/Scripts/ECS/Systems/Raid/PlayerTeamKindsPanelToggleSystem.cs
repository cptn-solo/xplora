using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class PlayerTeamKindsPanelToggleSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EntityButtonClickedTag> clickedPool = default;

        private readonly EcsPoolInject<ToggleTag<ExpandKindsTag>> panelTogglePool = default;
        private readonly EcsFilterInject<
            Inc<ToggleTag<ExpandKindsTag>>> panelExpandedToggleFilter = default;

        private readonly EcsPoolInject<ResetTag<CollapseKindsTag>> panelResetPool = default;
        private readonly EcsFilterInject<
            Inc<ResetTag<CollapseKindsTag>>> panelResetFilter = default;

        private readonly EcsPoolInject<UpdateTag<ExpandKindsTag>> panelUpdatePool = default;
        private readonly EcsFilterInject<
            Inc<UpdateTag<ExpandKindsTag>>> panelUpdateFilter = default;


        private readonly EcsFilterInject<
            Inc<EntityButtonRef<ExpandKindsTag>, EntityButtonClickedTag>> expandClickedfilter = default;

        private readonly EcsFilterInject<
            Inc<EntityButtonRef<CollapseKindsTag>, EntityButtonClickedTag>> collapseClickedfilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            var expanded = panelExpandedToggleFilter.Value.GetEntitiesCount() > 0;
            ToggleByType<CollapseKindsTag>(world, expanded);
            ToggleByType<ExpandKindsTag>(world, !expanded);

            foreach (var entity in collapseClickedfilter.Value)
            {
                clickedPool.Value.Del(entity);

                ToggleByType<CollapseKindsTag>(world, false);
                ToggleByType<ExpandKindsTag>(world, true);

                if (panelResetFilter.Value.GetEntitiesCount() == 0)
                    panelResetPool.Value.Add(world.NewEntity());

                foreach (var toggle in panelExpandedToggleFilter.Value)
                    panelTogglePool.Value.Del(toggle);
            }

            foreach (var entity in expandClickedfilter.Value)
            {
                clickedPool.Value.Del(entity);
                ToggleByType<CollapseKindsTag>(world, true);
                ToggleByType<ExpandKindsTag>(world, false);


                if (panelUpdateFilter.Value.GetEntitiesCount() == 0)
                    panelUpdatePool.Value.Add(world.NewEntity());

                if (panelExpandedToggleFilter.Value.GetEntitiesCount() == 0)
                    panelTogglePool.Value.Add(world.NewEntity());
            }
        }

        private void ToggleByType<B> (EcsWorld world, bool toggle) where B : struct
        {
            var filter = world.Filter<EntityButtonRef<B>>().End();
            var pool = world.GetPool<EntityButtonRef<B>>();
            foreach (var entity in filter)
            {
                ref var buttonRef = ref pool.Get(entity);
                buttonRef.EntityButton.Toggle(toggle);
            }
        }
    }
}