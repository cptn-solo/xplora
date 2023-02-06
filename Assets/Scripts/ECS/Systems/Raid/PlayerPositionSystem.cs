using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerPositionSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<FieldCellComp> cellPool;

        private readonly EcsFilterInject<Inc<PlayerComp>, Exc<FieldCellComp>> positionFilter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            var count = positionFilter.Value.GetEntitiesCount();

            if (count <= 0)
                return;

            int[] freeCellsIndexes = worldService.Value
                .GetRandomFreeCellIndexes(count);

            var i = -1;

            foreach (var entity in positionFilter.Value)
            {
                var cellIndex = freeCellsIndexes[++i];

                ref var cellComp = ref cellPool.Value.Add(entity);
                cellComp.CellIndex = cellIndex;
            }
        }
    }
}