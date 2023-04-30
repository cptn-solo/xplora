using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<DraftTag<BattleInfo>> draftTagPool = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;
        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;

        public void Init(IEcsSystems systems)
        {
            var entity = ecsWorld.Value.NewEntity();

            draftTagPool.Value.Add(entity);

            ref var battle = ref battleInfoPool.Value.Add(entity);
            battle.LastRoundNumber = -1;
            battle.LastTurnNumber = -1;
            battle.State = BattleState.Created;
            battle.QueuedRounds = new EcsPackedEntity[0];

            //battle.roundsQueue = new();
            //battle.currentTurn = BattleTurnInfo.Create(-1, Hero.Default, Hero.Default);

            battle.PlayerTeam = libraryService.Value.PlayerTeam;
            //battle.PlayerHeroes = new();

            battle.EnemyTeam = libraryService.Value.EnemyTeam;
            //battle.EnemyHeroes = new();

            battle.WinnerTeamId = -1;

            battleService.Value.BattleEntity = ecsWorld.Value.PackEntityWithWorld(entity);
        }

    }
}
