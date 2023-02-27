using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using System.Linq;
using UnityEditor.Sprites;

namespace Assets.Scripts.ECS.Systems
{
    using static Zenject.SignalSubscription;
    using HeroPosition = Tuple<int, BattleLine, int>;
    using Random = UnityEngine.Random;

    public class BattleHeroesInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool;
        private readonly EcsPoolInject<HeroInstanceOriginRefComp> heroInstanceOriginRefPool;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool;
        private readonly EcsPoolInject<EnemyTeamTag> enemyTeamTagPool;
        private readonly EcsPoolInject<PositionComp> positionPool;

        private readonly EcsCustomInject<BattleManagementService> battleService;
        private readonly EcsCustomInject<HeroLibraryService> libraryService;

        public void Init(IEcsSystems systems)
        {
            if (!battleService.Value.BattleEntity.Unpack(out var battleWorld, out var battleEntity))
                return;

            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);
            battleInfo.State = BattleState.PrepareTeams;
            battleService.Value.NotifyBattleEventListeners(battleInfo);

            #region Player team:

            var playerTeamId = battleInfo.PlayerTeam.Id;
            var playerPosBuffer = ListPool<HeroPosition>.Get();
            playerPosBuffer.AddRange(battleService.Value.BattleFieldSlotsPositions.
                Where(x => x.Item1 == playerTeamId));

            EcsPackedEntityWithWorld[] playerConfigs = battleService.Value.PlayerTeamPackedEntities;
                //GetHeroConfigsForTeam(playerTeamId);

            foreach (var packed in playerConfigs)
            {
                var sourcePosition = playerPosBuffer[0];

                AddHeroBattleInstance<PlayerTeamTag>(battleWorld, packed, sourcePosition);

                playerPosBuffer.RemoveAt(0);
            }

            ListPool<HeroPosition>.Add(playerPosBuffer);

            #endregion

            #region Enemy team:

            var enemyTeamId = battleInfo.EnemyTeam.Id;
            var enemyPosBuffer = ListPool<HeroPosition>.Get();
            enemyPosBuffer.AddRange(battleService.Value.BattleFieldSlotsPositions.
                Where(x => x.Item1 == enemyTeamId));
            EcsPackedEntityWithWorld[] enemyConfigs = battleService.Value.EnemyTeamPackedEntities;
                //GetHeroConfigsForTeam(enemyTeamId);

            EcsPackedEntityWithWorld? lastUsedEnemyConfig = null;

            foreach (var packed in enemyConfigs)
            {
                var sourcePosition = enemyPosBuffer[0];

                AddHeroBattleInstance<EnemyTeamTag>(battleWorld, packed, sourcePosition);

                lastUsedEnemyConfig = packed;

                enemyPosBuffer.RemoveAt(0);
            }

            if (battleWorld.Filter<EnemyTeamTag>().End().GetEntitiesCount() is int enemyTeamCount &&
                enemyTeamCount > 0 &&
                battleWorld.Filter<PlayerTeamTag>().End().GetEntitiesCount() is int playerTeamCount &&
                playerTeamCount > 0)
            {
                for (int i = 0; i < Random.Range(0, enemyPosBuffer.Count + 1); i++)
                {
                    var pos = enemyPosBuffer[Random.Range(0, enemyPosBuffer.Count)];

                    AddHeroBattleInstance<EnemyTeamTag>(battleWorld, lastUsedEnemyConfig.Value, pos);

                    enemyPosBuffer.Remove(pos);
                }
            }

            ListPool<HeroPosition>.Add(enemyPosBuffer);

            #endregion
        }

        private void AddHeroBattleInstance<T>(EcsWorld battleWorld,
            EcsPackedEntityWithWorld packed, HeroPosition sourcePosition)
            where T: struct
        {
            var entity = battleWorld.NewEntity();

            ref var positionComp = ref positionPool.Value.Add(entity);
            positionComp.Position = sourcePosition;

            battleWorld.GetPool<T>().Add(entity);

            // remember hero instance origin (raid or library)
            // to update HP and use bonuses from raid
            ref var origin = ref heroInstanceOriginRefPool.Value.Add(entity);
            origin.HeroInstancePackedEntity = packed;

            // getting hero config ref from the origin
            if (!packed.Unpack(out var originWorld, out var originEntity))
                throw new Exception("No Source Hero");

            ref var originConfigRefComp = ref originWorld.GetPool<HeroConfigRefComp>().Get(originEntity);
            ref var configRefComp = ref heroConfigRefPool.Value.Add(entity);
            configRefComp.HeroConfigPackedEntity = originConfigRefComp.Packed;
        }

        private EcsPackedEntityWithWorld[] GetHeroConfigsForTeam(int teamId)
        {
            var teamConfigs = ListPool<EcsPackedEntityWithWorld>.Get();
            
            foreach (var heroConfigPackedEntity in libraryService.Value.HeroConfigEntities)
            {
                //clone hero config into a new entity in the battle context
                //so we could use same config for several enemies for example
                if (!heroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                    throw new Exception("No Hero");

                var libPositionPool = libWorld.GetPool<PositionComp>();
                ref var libPosition = ref libPositionPool.Get(heroConfigEntity);

                HeroPosition sourcePosition = libPosition.Position;
                if (sourcePosition.Item1 == teamId)
                    teamConfigs.Add(heroConfigPackedEntity);
            }

            var retval = teamConfigs.ToArray();

            ListPool<EcsPackedEntityWithWorld>.Add(teamConfigs);

            return retval;
        }


    }
}
