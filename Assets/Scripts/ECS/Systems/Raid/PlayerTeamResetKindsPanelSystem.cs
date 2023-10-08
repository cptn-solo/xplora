using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamResetKindsPanelSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<ItemsContainerRef<HeroKindBarInfo>> containerRefPool = default;
        private readonly EcsPoolInject<ResetTag<CollapseKindsTag>> resetTagPool = default;
        private readonly EcsPoolInject<UpdateTag<HeroKindBarInfo>> updateTagPool = default;

        private readonly EcsFilterInject<
            Inc<ResetTag<CollapseKindsTag>>> triggerFilter;
        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, ItemsContainerRef<HeroKindBarInfo>>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var triggerEntity in triggerFilter.Value)
            {
                foreach (var entity in filter.Value)
                {
                    ref var containerRef = ref containerRefPool.Value.Get(entity);
                    containerRef.Container.Reset();
                    
                    if (updateTagPool.Value.Has(entity))
                        updateTagPool.Value.Del(entity);
                }

                resetTagPool.Value.Del(triggerEntity);
            }
        }
    }
}