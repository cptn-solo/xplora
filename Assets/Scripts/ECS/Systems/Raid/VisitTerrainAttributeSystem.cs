using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static Zenject.SignalSubscription;

namespace Assets.Scripts.ECS.Systems
{
    public class VisitTerrainAttributeSystem : IEcsRunSystem
    {
        //private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<VisitedComp<TerrainAttributeComp>> pool;
        private readonly EcsPoolInject<
            ActiveTraitHeroComp<TerrainAttributeComp>> traitHeroPool;

        private readonly EcsFilterInject<
            Inc<VisitedComp<TerrainAttributeComp>,
                ActiveTraitHeroComp<TerrainAttributeComp>>> visitFilter;

        private readonly EcsCustomInject<RaidService> raidService;
        private readonly EcsCustomInject<WorldService> worldService;

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

                raidService.Value.TryCastEcsTerrainEvent(eventConfig,
                    traitHero.Hero, traitHero.PackedHeroInstanceEntity, traitHero.MaxLevel);
            }
        }

    }
}