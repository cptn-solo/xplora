using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PresenceMonitorSystem : BaseEcsSystem
    {
        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<bool>>> factoryFilter = default;
        private readonly EcsCustomInject<WorldService> worldService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            if (!worldService.Value.WorldEntity.Unpack(out var world, out var worldEntity))
                return;

            switch (worldService.Value.WorldState)
            {
                case WorldState.AwaitingTerrain:
                    {
                        if (worldService.Value.TerrainProducer == null)
                            break;

                        if (factoryFilter.Value.GetEntitiesCount() == 0)
                            break;

                        worldService.Value.WorldState = WorldState.TerrainBeingGenerated;
                        world.GetPool<ProduceTag>().Add(worldEntity);

                    }
                    break;
                case WorldState.AwaitingTerrainDestruction:
                    {
                        worldService.Value.WorldState = WorldState.TerrainBeingDestoyed;
                        world.GetPool<DestroyTag>().Add(worldEntity);
                    }
                    break;
                default:
                    break;
            }            
        }
    }
}