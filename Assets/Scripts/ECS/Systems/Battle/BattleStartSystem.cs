﻿using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleStartSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInProgressTag> battleInProgressTagPool = default;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsFilterInject<
            Inc<BattleInfo>,
            Exc<BattleInProgressTag, BattleCompletedTag>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

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
