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
        private readonly EcsPoolInject<
            ActiveTraitHeroComp<WatchTowerComp>> traitHeroPool;

        private readonly EcsFilterInject<
            Inc<PlayerComp, FieldCellComp, VisitedComp<WatchTowerComp>>> visitFilter;

        private readonly EcsCustomInject<WorldService> worldService;
        private readonly EcsCustomInject<AudioPlaybackService> audioService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in visitFilter.Value)
            {
                ref var fieldCell = ref fieldCellPool.Value.Get(entity);

                var range = 5;
                if (traitHeroPool.Value.Has(entity))
                {
                    ref var traitHero = ref traitHeroPool.Value.Get(entity);
                    range += traitHero.MaxLevel;
                }

                worldService.Value.UnveilCellsInRange(fieldCell.CellIndex, range);
                audioService.Value.Play(CommonSoundEvent.WatchTower.SoundForEvent());
            }
        }
    }
}