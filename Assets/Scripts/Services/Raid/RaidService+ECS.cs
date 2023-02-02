using Leopotam.EcsLite;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite.Di;
using UnityEngine;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using System.Collections.Generic;

namespace Assets.Scripts.Services
{
    public partial class RaidService // ECS
    {
        private EcsWorld ecsRaidContext = null;
        private IEcsSystems ecsRaidSystems = null;

        public EcsPackedEntity PlayerEntity { get; set; }
        public EcsPackedEntity WorldEntity { get; set; }
        public EcsPackedEntity RaidEntity { get; set; }
        public EcsPackedEntity BattleEntity { get; set; }


        /// <summary>
        /// Run before new raid, not after return from the battle/event
        /// </summary>
        private void InitEcsRaidContext()
        {
            ecsRaidContext = new EcsWorld();
            ecsRaidSystems = new EcsSystems(ecsRaidContext);

            ecsRaidSystems
                .Add(new PlayerInitSystem())
                .Add(new RaidInitSystem())
                .Add(new DeployUnitSystem())
                .Add(new DestroyUnitSystem())
                .Add(new BattleAftermathSystem())
                .Add(new RetireEnemySystem())
                .Add(new RetirePlayerSystem())
                .Add(new VisitSystem())
                .Add(new LeaveSystem())
                .Add(new RefillSystem())
                .Add(new DrainSystem())
                .Add(new GarbageCollectorSystem())
#if UNITY_EDITOR
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Init();

        }
        /// <summary>
        /// Run on raid completion (return to the library after raid was lost)
        /// </summary>
        private void DestroyEcsRaidContext()
        {
            ecsRaidSystems?.Destroy();
            ecsRaidSystems = null;

            ecsRaidContext?.Destroy();
            ecsRaidContext = null;
        }

        private void StartEcsRaidContext()
        {
            if (ecsRaidContext == null)
                InitEcsRaidContext();

            raidRunloopCoroutine ??= StartCoroutine(RaidRunloopCoroutine());
        }

        private void StopEcsRaidContext()
        {
            if (raidRunloopCoroutine != null)
                StopCoroutine(raidRunloopCoroutine);

            raidRunloopCoroutine = null;

            if (ecsRaidContext != null)
                DestroyEcsRaidContext();
        }


        private Unit DeployEcsWorldPlayer(DeployWorldUnit callback)
        {
            if (!PlayerEntity.Unpack(ecsRaidContext, out var playerEntity))
                return null;

            var playerPool = ecsRaidContext.GetPool<PlayerComp>();
            ref var playerComp = ref playerPool.Get(playerEntity);

            if (playerComp.Hero.HeroType == HeroType.NA)
                return null;

            if (playerComp.CellIndex < 0)
                playerComp.CellIndex = Random.Range(0, worldService.CellCount);

            var playerUnit = callback(playerComp.CellIndex, playerComp.Hero);

            var unitPool = ecsRaidContext.GetPool<UnitComp>();
            ref var unitComp = ref unitPool.Add(playerEntity);
            unitComp.Unit = playerUnit;

            return playerUnit;

        }

        private void DeployEcsWorldOpponents(DeployWorldUnit callback)
        {
            var opponentFilter = ecsRaidContext.Filter<OpponentComp>().End();
            var opponentPool = ecsRaidContext.GetPool<OpponentComp>();
            var playerPool = ecsRaidContext.GetPool<PlayerComp>();

            if (!PlayerEntity.Unpack(ecsRaidContext, out var playerEntity))
                return;

            ref var playerComp = ref playerPool.Get(playerEntity);
            var usedCells = new List<int>
            {
                playerComp.CellIndex
            };

            foreach (var opponentEntity in opponentFilter)
            {
                ref var opponentComp = ref opponentPool.Get(opponentEntity);
                var enemyHero = opponentComp.Hero;

                if (opponentComp.CellIndex < 0)
                {
                    var cellCount = worldService.CellCount;
                    var cellIndex = Random.Range(0, cellCount);
                    while (usedCells.Contains(cellIndex))
                    {
                        cellIndex = Random.Range(0, cellCount);
                    }
                    opponentComp.CellIndex = cellIndex;
                    usedCells.Add(cellIndex);
                }

                if (enemyHero.HeroType != HeroType.NA)
                {
                    _ = callback(opponentComp.CellIndex, enemyHero);

                    var unitPool = ecsRaidContext.GetPool<UnitComp>();
                    ref var unitComp = ref unitPool.Add(opponentEntity);
                    unitComp.Unit = null;
                }

            }
        }
        private void DestroyEcsUnits(DestroyUnitsCallback callback)
        {
            // TODO: mark all unitComps for destruction
            var unitPool = ecsRaidContext.GetPool<UnitComp>();
            var unitFilter = ecsRaidContext.Filter<UnitComp>().End();
            foreach(var entity in unitFilter)
            {
                ref var unitComp = ref unitPool.Get(entity);
                unitComp.Unit = null;

                unitPool.Del(entity);
            }
            callback();
        }

        private void ProcessEcsBattleAftermath(bool won)
        {
            if (!BattleEntity.Unpack(ecsRaidContext, out var battleEntity))
                return;

            var aftermathPool = ecsRaidContext.GetPool<BattleAftermathComp>();
            ref var aftermathComp = ref aftermathPool.Add(battleEntity);
            aftermathComp.Won = won;
        }

        private void UpdateEcsPlayerCellId(int cellId)
        {
            if (!PlayerEntity.Unpack(ecsRaidContext, out var playerEntity))
                return;

            var playerPool = ecsRaidContext.GetPool<PlayerComp>();
            ref var playerComp = ref playerPool.Get(playerEntity);
            playerComp.CellIndex = cellId;
        }

        private bool CheckEcsWorldForOpponent(
            int cellId,
            out Hero enemyHero)
        {
            var opponentFilter = ecsRaidContext.Filter<OpponentComp>().End();
            var opponentPool = ecsRaidContext.GetPool<OpponentComp>();

            int enemyEntity = -1;
            enemyHero = default;
            foreach (var opponentEntity in opponentFilter)
            {
                ref var opponentComp = ref opponentPool.Get(opponentEntity);
                if (opponentComp.CellIndex == cellId)
                {
                    enemyEntity = opponentEntity;
                    enemyHero = opponentComp.Hero;
                    break;
                }
            }

            if (enemyEntity == -1)
                return false;

            var battlePool = ecsRaidContext.GetPool<BattleComp>();
            var battleEntity = ecsRaidContext.NewEntity();

            ref var battleComp = ref battlePool.Add(battleEntity);
            battleComp.EnemyPackedEntity = ecsRaidContext.PackEntity(enemyEntity);

            BattleEntity = ecsRaidContext.PackEntity(battleEntity);

            var unitPool = ecsRaidContext.GetPool<UnitComp>();
            var unitFilter = ecsRaidContext.Filter<UnitComp>().End();

            foreach (var unitEntity in unitFilter)
                unitPool.Del(unitEntity);

            return true;
        }        
    }
}

