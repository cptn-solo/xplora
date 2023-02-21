using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleStartSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInProgressTag> battleInProgressTagPool;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsFilterInject<Inc<BattleInfo>, Exc<BattleInProgressTag>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            if (battleService.Value.PlayMode != BattleMode.Autoplay)
                return;

            foreach (var entity in filter.Value)
            {
                battleInProgressTagPool.Value.Add(entity);

                ref var battleInfo = ref battleInfoPool.Value.Get(entity);
                battleInfo.State = BattleState.BattleInProgress;

                battleService.Value.NotifyBattleEventListeners();
            }
        }
    }
}
