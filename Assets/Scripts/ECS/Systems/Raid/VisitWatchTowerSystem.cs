using Assets.Scripts.Services;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class VisitWatchTowerSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<FieldCellComp> fieldCellPool;

        private readonly EcsFilterInject<
            Inc<PlayerComp, FieldCellComp, VisitedComp<WatchTowerComp>>> visitFilter;

        private readonly EcsCustomInject<WorldService> worldService;
        private readonly EcsCustomInject<AudioPlaybackService> audioService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in visitFilter.Value)
            {
                ref var fieldCell = ref fieldCellPool.Value.Get(entity);

                worldService.Value.UnveilCellsInRange(fieldCell.CellIndex, 5);
                audioService.Value.Play(CommonSoundEvent.WatchTower.SoundForEvent());
            }
        }
    }
}