using System.Collections.Generic;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class RaidInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<RaidComp> raidPool;
        private readonly EcsPoolInject<OpponentComp> opponentPool;
        private readonly EcsPoolInject<HeroComp> heroPool;
        private readonly EcsPoolInject<FieldCellComp> cellPool;
        private readonly EcsPoolInject<POIComp> poiPool;

        private readonly EcsFilterInject<Inc<PlayerComp>> playerFilter;
        private readonly EcsFilterInject<Inc<OpponentComp>> opponentFilter;

        private readonly EcsCustomInject<RaidService> raidService;
        private readonly EcsCustomInject<WorldService> worldService;

        public void Init(IEcsSystems systems)
        {
            var success = raidService.Value.AssignPlayerAndEnemies(
                out var playerHeroes,
                out var opponentHeroes);

            var raidEntity = ecsWorld.Value.NewEntity();

            ref var raidComp = ref raidPool.Value.Add(raidEntity);
            raidComp.InitialPlayerHeroes = playerHeroes;
            raidComp.InitialOpponentHeroes = opponentHeroes;

            raidService.Value.RaidEntity = ecsWorld.Value.PackEntity(raidEntity);

            SyncEcsRaidParties(playerHeroes, opponentHeroes);
            PositionEcsRaidParties();
        }

        private void PositionEcsRaidParties()
        {
            var opponentCount = opponentFilter.Value.GetEntitiesCount();
            var playerCount = playerFilter.Value.GetEntitiesCount();
            int[] freeCellsIndexes = worldService.Value
                .GetRandomFreeCellIndexes(opponentCount + playerCount);

            int i = -1;
            foreach (var entity in playerFilter.Value)
            {
                ref var cellComp = ref cellPool.Value.Add(entity);
                cellComp.CellIndex = freeCellsIndexes[++i];
            }

            foreach (var entity in opponentFilter.Value)
            {
                var cellIndex = freeCellsIndexes[++i];
                worldService.Value.AddPoi<OpponentComp>(
                    cellIndex,
                    ecsWorld.Value.PackEntityWithWorld(entity));

                ref var cellComp = ref cellPool.Value.Add(entity);
                cellComp.CellIndex = cellIndex;
            }
        }

        private void SyncEcsRaidParties(
            Hero[] playerHeroes,
            Hero[] freeHeroes)
        {
            foreach (var entity in playerFilter.Value)
            {
                ref var heroComp = ref heroPool.Value.Add(entity);
                heroComp.Hero = playerHeroes.Length > 0 ?
                    playerHeroes[0] : default;
            }

            foreach (var opponentHero in freeHeroes)
            {
                var opponentEntity = ecsWorld.Value.NewEntity();
                ref var opponentComp = ref opponentPool.Value.Add(opponentEntity);
                ref var heroComp = ref heroPool.Value.Add(opponentEntity);
                heroComp.Hero = opponentHero;
            }
        }


    }
}