﻿using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleHeroesInitSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<DraftTag<Hero>> draftHeroTagPool = default;
        private readonly EcsPoolInject<HeroConfigRef> heroConfigRefPool = default;
        private readonly EcsPoolInject<HeroInstanceOriginRef> heroInstanceOriginRefPool = default;
        private readonly EcsPoolInject<PositionComp> positionPool = default;
        private readonly EcsPoolInject<BattleFieldComp> battleFieldPool = default;
        private readonly EcsPoolInject<HeroInstanceMapping> heroInstanceMappingPool = default;

        private readonly EcsFilterInject<Inc<BattleInfo, BattleFieldComp, DraftTag<BattleInfo>>> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            if (!battleService.Value.BattleEntity.Unpack(out var battleWorld, out var battleEntity))
                return;

            if (filter.Value.GetEntitiesCount() == 0)
                return;

            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);            
            battleInfo.State = BattleState.PrepareTeams;
            battleService.Value.NotifyBattleEventListeners(battleInfo);

            ref var battleField = ref battleFieldPool.Value.Get(battleEntity);
            ref var heroInstanceMappings = ref heroInstanceMappingPool.Value.Get(battleEntity);

            #region Player team:

            var playerTeamId = battleInfo.PlayerTeam.Id;
            var playerPosBuffer = ListPool<HeroPosition>.Get();
            playerPosBuffer.AddRange(battleField.SlotPositions.
                Where(x => x.Item1 == playerTeamId));

            EcsPackedEntityWithWorld[] playerConfigs = battleService.Value.PlayerTeamPackedEntities;

            foreach (var packed in playerConfigs)
            {
                var sourcePosition = playerPosBuffer[0];

                AddHeroBattleInstance<PlayerTeamTag>(battleWorld, heroInstanceMappings, packed, sourcePosition);

                playerPosBuffer.RemoveAt(0);
            }

            ListPool<HeroPosition>.Add(playerPosBuffer);

            #endregion

            #region Enemy team:

            var enemyTeamId = battleInfo.EnemyTeam.Id;
            var enemyPosBuffer = ListPool<HeroPosition>.Get();
            enemyPosBuffer.AddRange(battleField.SlotPositions.
                Where(x => x.Item1 == enemyTeamId));
            EcsPackedEntityWithWorld[] enemyConfigs = battleService.Value.EnemyTeamPackedEntities;

            foreach (var packed in enemyConfigs)
            {
                var sourcePosition = enemyPosBuffer[0];

                AddHeroBattleInstance<EnemyTeamTag>(battleWorld, heroInstanceMappings, packed, sourcePosition);

                enemyPosBuffer.RemoveAt(0);
            }

            ListPool<HeroPosition>.Add(enemyPosBuffer);

            #endregion
        }

        private void AddHeroBattleInstance<T>(
            EcsWorld battleWorld,
            HeroInstanceMapping heroInstanceMappings,
            EcsPackedEntityWithWorld originHeroInstancePacked, 
            HeroPosition sourcePosition)
            where T: struct
        {
            var entity = battleWorld.NewEntity();

            draftHeroTagPool.Value.Add(entity);

            ref var positionComp = ref positionPool.Value.Add(entity);
            positionComp.Position = sourcePosition;

            battleWorld.GetPool<T>().Add(entity);

            // remember hero instance origin (raid or library)
            // to update HP and use bonuses from raid
            ref var originRef = ref heroInstanceOriginRefPool.Value.Add(entity);
            originRef.HeroInstancePackedEntity = originHeroInstancePacked;

            // getting hero config ref from the origin
            if (!originHeroInstancePacked.Unpack(out var originWorld, out var originEntity))
                throw new Exception("No Source Hero");

            ref var originConfigRefComp = ref originWorld.GetPool<HeroConfigRef>().Get(originEntity);
            ref var configRefComp = ref heroConfigRefPool.Value.Add(entity);
            configRefComp.HeroConfigPackedEntity = originConfigRefComp.Packed;

            var battleHeroInstancePacked = battleWorld.PackEntityWithWorld(entity);
            heroInstanceMappings.OriginToBattleMapping.Add(originHeroInstancePacked, battleHeroInstancePacked);
            heroInstanceMappings.BattleToOriginMapping.Add(battleHeroInstancePacked, originHeroInstancePacked);
        }
    }
}
