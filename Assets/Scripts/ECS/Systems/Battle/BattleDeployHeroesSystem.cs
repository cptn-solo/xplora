using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.UI.Battle;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDeployHeroesSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<EntityViewFactoryRef<Hero>> factoryPool;
        private readonly EcsPoolInject<PositionComp> positionPool;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool;
        private readonly EcsPoolInject<EntityViewRef<Hero>> entityViewRefPool;
        private readonly EcsPoolInject<BattleFieldComp> battleFieldPool;

        private readonly EcsFilterInject<Inc<EntityViewFactoryRef<Hero>>> factoryFilter;
        private readonly EcsFilterInject<
            Inc<HeroConfigRefComp, PositionComp>,
            Exc<EntityViewRef<Hero>, DeadTag>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            if (!battleService.Value.BattleEntity.Unpack(out var world, out var battleEntity))
                throw new Exception("No battle");

            foreach (var factoryEntity in factoryFilter.Value)
            {
                ref var factoryRef = ref factoryPool.Value.Get(factoryEntity);
                ref var battleField = ref battleFieldPool.Value.Get(battleEntity);

                foreach (var entity in filter.Value)
                {
                    ref var pos = ref positionPool.Value.Get(entity);
                    var slot = battleField.Slots[pos.Position];

                    var packed = ecsWorld.Value.PackEntityWithWorld(entity);

                    var card = (BattleUnit)factoryRef.FactoryRef(packed);
                    card.DataLoader = battleService.Value.GetHeroConfigForPackedEntity<Hero>;
                    card.IsPlayerTeam = playerTeamTagPool.Value.Has(entity);
                    slot.Put(card.Transform);
                    card.UpdateData();

                    ref var entityViewRef = ref entityViewRefPool.Value.Add(entity);
                    entityViewRef.EntityView = card;
                }
            }
        }

    }
}
