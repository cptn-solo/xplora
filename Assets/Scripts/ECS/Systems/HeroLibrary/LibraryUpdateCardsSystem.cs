using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryUpdateCardsSystem : IEcsRunSystem
    {        
        private readonly EcsPoolInject<UpdateTag> updateTagPool = default;
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool = default;
        private readonly EcsFilterInject<
            Inc<Hero, EntityViewRef<Hero>, UpdateTag>> filter = default;

        public void Run(IEcsSystems systems)
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

