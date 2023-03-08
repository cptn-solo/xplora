using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<DraftTag> draftTagPool;

        private readonly EcsCustomInject<HeroLibraryService> libraryService;

        public void Init(IEcsSystems systems)
        {
            var libEntity = ecsWorld.Value.NewEntity();
            draftTagPool.Value.Add(libEntity);
            libraryService.Value.LibraryEntity = ecsWorld.Value.PackEntityWithWorld(libEntity);
        }
    }
}

