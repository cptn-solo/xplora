using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class VisitTerrainAttributeSystem : IEcsRunSystem
    {
        //private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<VisitedComp<TerrainAttributeComp>> pool = default;
        private readonly EcsPoolInject<
            ActiveTraitHeroComp<TerrainAttributeComp>> traitHeroPool = default;

        private readonly EcsFilterInject<
            Inc<VisitedComp<TerrainAttributeComp>,
                ActiveTraitHeroComp<TerrainAttributeComp>>> visitFilter = default;

        private readonly EcsCustomInject<RaidService> raidService = default;
        private readonly EcsCustomInject<WorldService> worldService = default;

        public void Run(IEcsSystems systems)
        {
            if (raidService.Value.Dialog == null)
                return;

            foreach (var entity in visitFilter.Value)
            {
                ref var attributes = ref pool.Value.Get(entity);
                ref var traitHero = ref traitHeroPool.Value.Get(entity);

                //NB: for now it is 1:1 mapping, can be extended later
                var eventConfig = worldService.Value
                    .TerrainEventsLibrary.TerrainEvents[attributes.Info.TerrainAttribute];

                raidService.Value.CastEcsTerrainEvent(eventConfig,
                    traitHero.Hero, traitHero.PackedHeroInstanceEntity, traitHero.MaxLevel);
            }
        }

    }
}