using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class TeamInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<Team> teamPool = default;
        private readonly EcsPoolInject<UpdateAssetBalanceTag> updatePool = default;

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;

        public void Init(IEcsSystems systems)
        {
            var playerTeamEntity = ecsWorld.Value.NewEntity();

            ref Team playerTeam = ref teamPool.Value.Add(playerTeamEntity);
            playerTeam.Id = 0;
            playerTeam.Name = "Player";
            playerTeam.Assets = new Asset[0];

            updatePool.Value.Add(playerTeamEntity);

            libraryService.Value.PlayerTeamEntity = ecsWorld.Value.PackEntityWithWorld(playerTeamEntity);

            var enemyTeamEntity = ecsWorld.Value.NewEntity();

            ref Team enemyTeam = ref teamPool.Value.Add(enemyTeamEntity);
            enemyTeam.Id = 1;
            enemyTeam.Name = "Enemy";

            libraryService.Value.EnemyTeamEntity = ecsWorld.Value.PackEntityWithWorld(enemyTeamEntity);


        }
    }
}

