using System.Collections.Generic;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class RaidInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<RaidComp> raidPool;
        private readonly EcsPoolInject<PlayerComp> playerPool;
        private readonly EcsPoolInject<OpponentComp> opponentPool;

        private readonly EcsFilterInject<Inc<OpponentComp>> opponentFilter;

        private readonly EcsCustomInject<RaidService> raidService;

        public void Init(IEcsSystems systems)
        {
            var raidEntity = ecsWorld.Value.NewEntity();

            if (!raidService.Value.AssignPlayerAndEnemies(
                out var playerHeroes,
                out var opponentHeroes))
                return;

            ref var raidComp = ref raidPool.Value.Add(raidEntity);
            raidComp.InitialPlayerHeroes = playerHeroes;
            raidComp.InitialOpponentHeroes = opponentHeroes;

            raidService.Value.RaidEntity = ecsWorld.Value.PackEntity(raidEntity);

            SyncEcsRaidParties(playerHeroes, opponentHeroes);
        }

        private void SyncEcsRaidParties(
            Hero[] playerHeroes,
            Hero[] freeHeroes)
        {
            // reassign avatar hero to player entity
            if (raidService.Value.PlayerEntity.Unpack(
                ecsWorld.Value, out var playerEntity))
            {
                ref var playerComp = ref playerPool.Value.Get(playerEntity);

                playerComp.Hero = playerHeroes.Length > 0 ?
                    playerHeroes[0] : default;

                playerComp.CellIndex = -1;
            }

            // clear opponents' entities
            foreach (var opponentEntity in opponentFilter.Value)
            {
                ecsWorld.Value.DelEntity(opponentEntity);
            }

            // create new ones marked with enemies' avatars
            foreach (var opponentHero in freeHeroes)
            {
                var opponentEntity = ecsWorld.Value.NewEntity();
                ref var opponentComp = ref opponentPool.Value.Add(opponentEntity);
                opponentComp.Hero = opponentHero;
                opponentComp.CellIndex = -1;
            }
        }


    }
}