using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleRetreatSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInProgressTag> battleInProgressTagPool = default;
        private readonly EcsPoolInject<WinnerTag> winnerTagPool = default;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;

        private readonly EcsFilterInject<
            Inc<BattleInfo, RetreatTag>,
            Exc<WinnerTag>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

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

            if (battleInfo.State switch {
                BattleState.TeamsPrepared => false,
                BattleState.BattleStarted => false,
                BattleState.BattleInProgress => false,
                _ => true })
                return;

            battleInfo.SetWinnerTeamId(battleInfo.EnemyTeam.Id);

            battleInProgressTagPool.Value.Del(battleEntity);
            winnerTagPool.Value.Add(battleEntity);
        }
    }
}
