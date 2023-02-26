using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleWinCheckSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;

        private readonly EcsPoolInject<WinnerTag> winnerTagPool;
        private readonly EcsPoolInject<BattleInProgressTag> battleInProgressTagPool;
        
        private readonly EcsFilterInject<
            Inc<BattleInfo, BattleInProgressTag>,
            Exc<WinnerTag>> filter;
        private readonly EcsFilterInject<Inc<PlayerTeamTag>, Exc<DeadTag>> playerHeroes;
        private readonly EcsFilterInject<Inc<EnemyTeamTag>, Exc<DeadTag>> enemyHeroes;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                if (playerHeroes.Value.GetEntitiesCount() == 0)
                    BattleWon(entity, false);

                if (enemyHeroes.Value.GetEntitiesCount() == 0)
                    BattleWon(entity, true);

            }            
        }

        private void BattleWon(int battleEntity, bool player)
        {
            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);
            battleInfo.WinnerTeamId = player ?
                battleInfo.PlayerTeam.Id :
                battleInfo.EnemyTeam.Id;

            battleInProgressTagPool.Value.Del(battleEntity);
            winnerTagPool.Value.Add(battleEntity);
        }
    }
}
