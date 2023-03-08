using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Linq;

namespace Assets.Scripts.ECS.Systems
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public class BattleHeroesInitSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsPoolInject<DraftTag<BattleInfo>> draftBattleTagPool;
        private readonly EcsPoolInject<DraftTag<Hero>> draftHeroTagPool;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool;
        private readonly EcsPoolInject<HeroInstanceOriginRefComp> heroInstanceOriginRefPool;
        private readonly EcsPoolInject<PositionComp> positionPool;
        private readonly EcsPoolInject<BattleFieldComp> battleFieldPool;

        private readonly EcsFilterInject<Inc<BattleInfo, BattleFieldComp, DraftTag<BattleInfo>>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            if (!battleService.Value.BattleEntity.Unpack(out var battleWorld, out var battleEntity))
                return;

            if (filter.Value.GetEntitiesCount() == 0)
                return;

            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);            
            battleInfo.State = BattleState.PrepareTeams;
            battleService.Value.NotifyBattleEventListeners(battleInfo);

            ref var battleField = ref battleFieldPool.Value.Get(battleEntity);

            #region Player team:

            var playerTeamId = battleInfo.PlayerTeam.Id;
            var playerPosBuffer = ListPool<HeroPosition>.Get();
            playerPosBuffer.AddRange(battleField.SlotPositions.
                Where(x => x.Item1 == playerTeamId));

            EcsPackedEntityWithWorld[] playerConfigs = battleService.Value.PlayerTeamPackedEntities;

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
            enemyPosBuffer.AddRange(battleField.SlotPositions.
                Where(x => x.Item1 == enemyTeamId));
            EcsPackedEntityWithWorld[] enemyConfigs = battleService.Value.EnemyTeamPackedEntities;

            foreach (var packed in enemyConfigs)
            {
                var sourcePosition = enemyPosBuffer[0];

                AddHeroBattleInstance<EnemyTeamTag>(battleWorld, packed, sourcePosition);

                enemyPosBuffer.RemoveAt(0);
            }

            ListPool<HeroPosition>.Add(enemyPosBuffer);

            #endregion
        }

        private void AddHeroBattleInstance<T>(EcsWorld battleWorld,
            EcsPackedEntityWithWorld packed, HeroPosition sourcePosition)
            where T: struct
        {
            var entity = battleWorld.NewEntity();

            draftHeroTagPool.Value.Add(entity);

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
    }
}
