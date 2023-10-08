using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamTriggerKindsPanelUpdateSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<UpdateTag<ExpandKindsTag>> triggerPool = default;
        private readonly EcsPoolInject<UpdateTag<HeroKindBarInfo>> updateTagPool = default;

        private readonly EcsFilterInject<Inc<UpdateTag<ExpandKindsTag>>> triggerFilter = default;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, ItemsContainerRef<HeroKindBarInfo>>,
            Exc<UpdateTag<HeroKindBarInfo>>> filter = default;

        public override void RunIfActive(IEcsSystems systems)        {
            foreach (var trigger in triggerFilter.Value)
            {
                foreach (var entity in filter.Value)
                    updateTagPool.Value.Add(entity);

                triggerPool.Value.Del(trigger);
            }
        }
    }
}