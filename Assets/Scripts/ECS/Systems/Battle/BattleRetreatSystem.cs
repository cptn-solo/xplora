using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleRetreatSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInProgressTag> battleInProgressTagPool;
        private readonly EcsPoolInject<WinnerTag> winnerTagPool;
        private readonly EcsPoolInject<RetreatTag> retreatTagPool;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsFilterInject<Inc<BattleInfo, BattleInProgressTag>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            if (battleService.Value.PlayMode != BattleMode.NA)
                return;

            foreach (var entity in filter.Value)
                RetreatBattle(entity);            
        }

        private void RetreatBattle(int battleEntity)
        {
            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);

            if (battleInfo.State == BattleState.BattleStarted ||
                battleInfo.State == BattleState.BattleInProgress)
            {
                battleInfo.SetWinnerTeamId(battleInfo.EnemyTeam.Id);

                battleInProgressTagPool.Value.Del(battleEntity);
                winnerTagPool.Value.Add(battleEntity);
                retreatTagPool.Value.Add(battleEntity);

                //yield return new WaitForSeconds(2.0f);
            }
        }
    }
}
