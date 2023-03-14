﻿using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleWinCheckSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;

        private readonly EcsPoolInject<WinnerTag> winnerTagPool = default;
        private readonly EcsPoolInject<BattleInProgressTag> battleInProgressTagPool = default;
        
        private readonly EcsFilterInject<
            Inc<BattleInfo, BattleInProgressTag>,
            Exc<WinnerTag>> filter = default;
        private readonly EcsFilterInject<Inc<PlayerTeamTag>, Exc<DeadTag>> playerHeroes = default;
        private readonly EcsFilterInject<Inc<EnemyTeamTag>, Exc<DeadTag>> enemyHeroes = default;

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
