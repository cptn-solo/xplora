using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;

namespace Assets.Scripts.Services
{
    public partial class WorldService // ECS
    {
        public EcsPackedEntity WorldEntity { get; set; }

        private EcsWorld ecsWorld;
        private IEcsSystems ecsSystems;

        private void StartEcsWorldContext()
        {
            ecsWorld = new EcsWorld();
            ecsSystems = new EcsSystems(ecsWorld);

            ecsSystems
                .Add(new TerrainGenerationSystem())
                .Add(new TerrainDestructionSystem())
                .Add(new GarbageCollectorSystem())
#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Init();

            var worldEntity = ecsWorld.NewEntity();

            var worldPool = ecsWorld.GetPool<WorldComp>();
            ref var worldComp = ref worldPool.Add(worldEntity);

            WorldEntity = ecsWorld.PackEntity(worldEntity);
        }

        private void StopEcsWorldContext()
        {
            ecsSystems?.Destroy();
            ecsSystems = null;

            ecsWorld?.Destroy();
            ecsWorld = null;
        }

        private void ProduceEcsWorld()
        {
            if (!WorldEntity.Unpack(ecsWorld, out var worldEntity))
                return;

            ecsWorld.GetPool<ProduceTag>().Add(worldEntity);
        }

        private void DestroyEcsWorld()
        {
            if (!WorldEntity.Unpack(ecsWorld, out var worldEntity))
                return;

            ecsWorld.GetPool<DestroyTag>().Add(worldEntity);

        }
    }

}

