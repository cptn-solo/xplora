using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryUpdateCardsSystem : BaseEcsSystem
    {        
        private readonly EcsPoolInject<UpdateTag> updateTagPool = default;
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool = default;
        private readonly EcsFilterInject<
            Inc<EntityViewRef<Hero>, UpdateTag>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var entityViewRef = ref entityViewRefPool.Value.Get(entity);
                entityViewRef.EntityView.UpdateData();

                updateTagPool.Value.Del(entity);
            }
        }
    }
}

