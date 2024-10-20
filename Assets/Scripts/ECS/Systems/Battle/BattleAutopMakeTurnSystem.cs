﻿using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleAutoMakeTurnSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<MakeTurnTag> makeTurnTagPool = default;
        private readonly EcsPoolInject<ReadyTurnTag> readyTurnTagPool = default;
        private readonly EcsPoolInject<AttackTag> attackTagPool = default;

        private readonly EcsFilterInject<Inc<BattleInfo, BattleInProgressTag>> battleFilter = default;
        private readonly EcsFilterInject<Inc<BattleTurnInfo, ReadyTurnTag>> turnInfoFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            if (battleFilter.Value.GetEntitiesCount() == 0)
                return;

            var autoplay = battleService.Value.PlayMode switch
            {
                BattleMode.Autoplay => true,
                BattleMode.Fastforward => true,
                _ => false
            };

            if (!autoplay)
                return;

            foreach (var entity in turnInfoFilter.Value)
            {
                //Debug.Break();
                makeTurnTagPool.Value.Add(entity);
                attackTagPool.Value.Add(entity);
                readyTurnTagPool.Value.Del(entity);
            }

        }
    }
}
