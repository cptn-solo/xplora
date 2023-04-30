using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerPositionSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<FieldCellComp> cellPool = default;

        private readonly EcsFilterInject<Inc<PlayerComp>, Exc<FieldCellComp>> positionFilter = default;

        private readonly EcsCustomInject<WorldService> worldService = default;

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