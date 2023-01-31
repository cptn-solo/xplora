using UnityEngine;
using System.Collections;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using Assets.Scripts.World;

namespace Assets.Scripts.Services
{
    public partial class WorldService // ECS
    {
        public EcsPackedEntity PlayerEntity { get; set; }
        public EcsPackedEntity WorldEntity { get; set; }
        public EcsPackedEntity RaidEntity { get; set; }
        public EcsPackedEntity BattleEntity { get; set; }

        private EcsWorld ecsWorld;
        private IEcsSystems ecsSystems;

        private EcsWorld ecsRaidContext;
        private IEcsSystems ecsRaidSystems;


        private delegate bool LibraryRetireHero(Hero hero);
        private delegate Unit DeployWorldUnit(int cellId, Hero hero);

        private void StartEcsWorldContext()
        {
            ecsWorld = new EcsWorld();
            ecsSystems = new EcsSystems(ecsWorld);

            ecsSystems
                .Add(new WorldSystem())
                .Add(new PlayerSystem())
#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Init();

        }

        private void StopEcsWorldContext()
        {
            ecsSystems?.Destroy();
            ecsSystems = null;

            ecsWorld?.Destroy();
            ecsWorld = null;
        }


        private void StartEcsRaidContext()
        {
            ecsRaidContext = new EcsWorld();
            ecsRaidSystems = new EcsSystems(ecsRaidContext);

            ecsRaidSystems
                .Add(new RaidSystem())
                .Add(new VisitSystem())
                .Add(new LeaveSystem())
                .Add(new RefillSystem())
                .Add(new DrainSystem())
#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Init();
        }

        private void StopEcsRaidContext()
        {
            ecsRaidSystems?.Destroy();
            ecsRaidSystems = null;

            ecsRaidContext?.Destroy();
            ecsRaidContext = null;
        }


        private void SyncEcsRaidParties(
            Hero playerAvatar,
            IEnumerable<Hero> freeHeroes)
        {
            // reassign avatar hero to player entity
            if (PlayerEntity.Unpack(ecsWorld, out var playerEntity))
            {
                var playerPool = ecsWorld.GetPool<PlayerComp>();
                ref var playerComp = ref playerPool.Get(playerEntity);
                playerComp.Hero = playerAvatar;
                playerComp.CellIndex = -1;
            }

            // clear opponents' entities
            var opponentFilter = ecsWorld.Filter<OpponentComp>().End();
            foreach (var opponentEntity in opponentFilter)
            {
                ecsWorld.DelEntity(opponentEntity);
            }

            // create new ones marked with enemies' avatars
            var opponentPool = ecsWorld.GetPool<OpponentComp>();
            foreach (var opponentHero in freeHeroes)
            {
                var opponentEntity = ecsWorld.NewEntity();
                ref var opponentComp = ref opponentPool.Add(opponentEntity);
                opponentComp.Hero = opponentHero;
                opponentComp.CellIndex = -1;
            }
        }

        private bool CheckEcsWorldForOpponent(
            int cellId,
            out Hero enemyHero)
        {
            var opponentFilter = ecsWorld.Filter<OpponentComp>().End();
            var opponentPool = ecsWorld.GetPool<OpponentComp>();

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

            var battlePool = ecsWorld.GetPool<BattleComp>();
            var battleEntity = ecsWorld.NewEntity();

            ref var battleComp = ref battlePool.Add(battleEntity);
            battleComp.EnemyPackedEntity = ecsWorld.PackEntity(enemyEntity);

            BattleEntity = ecsWorld.PackEntity(battleEntity);

            var unitPool = ecsWorld.GetPool<UnitComp>();
            var unitFilter = ecsWorld.Filter<UnitComp>().End();

            foreach (var unitEntity in unitFilter)
                unitPool.Del(unitEntity);

            return true;
        }

        private Unit DeployEcsWorldPlayer(DeployWorldUnit callback)
        {
            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return null;

            var playerPool = ecsWorld.GetPool<PlayerComp>();
            ref var playerComp = ref playerPool.Get(playerEntity);

            if (playerComp.Hero.HeroType == HeroType.NA)
                return null;

            if (playerComp.CellIndex < 0)
                playerComp.CellIndex = Random.Range(0, width * height);

            var playerUnit = callback(playerComp.CellIndex, playerComp.Hero);

            var unitPool = ecsWorld.GetPool<UnitComp>();
            ref var unitComp = ref unitPool.Add(playerEntity);
            unitComp.Unit = playerUnit;

            return playerUnit;

        }

        private void DeployEcsWorldOpponents(DeployWorldUnit callback)
        {
            var opponentFilter = ecsWorld.Filter<OpponentComp>().End();
            var opponentPool = ecsWorld.GetPool<OpponentComp>();
            var playerPool = ecsWorld.GetPool<PlayerComp>();

            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return;

            ref var playerComp = ref playerPool.Get(playerEntity);
            var usedCells = new List<int>();
            usedCells.Add(playerComp.CellIndex);

            foreach (var opponentEntity in opponentFilter)
            {
                ref var opponentComp = ref opponentPool.Get(opponentEntity);

                var enemyHero = opponentComp.Hero;

                if (opponentComp.CellIndex < 0)
                {
                    var cellIndex = Random.Range(0, width * height);
                    while (usedCells.Contains(cellIndex))
                    {
                        cellIndex = Random.Range(0, width * height);
                    }
                    opponentComp.CellIndex = cellIndex;
                    usedCells.Add(cellIndex);
                }

                Unit enemyUnit = null;
                if (enemyHero.HeroType != HeroType.NA)
                {
                    enemyUnit = callback(opponentComp.CellIndex, enemyHero);

                    var unitPool = ecsWorld.GetPool<UnitComp>();
                    ref var unitComp = ref unitPool.Add(opponentEntity);
                    unitComp.Unit = enemyUnit;
                }

            }

        }

        private void UpdateEcsPlayerCellId(int cellId)
        {
            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return;

            var playerPool = ecsWorld.GetPool<PlayerComp>();
            ref var playerComp = ref playerPool.Get(playerEntity);
            playerComp.CellIndex = cellId;
        }

        private void CleanupEcsRaid(LibraryRetireHero retireHeroCallback)
        {
            var unitPool = ecsWorld.GetPool<UnitComp>();

            var playerPool = ecsWorld.GetPool<PlayerComp>();
            if (PlayerEntity.Unpack(ecsWorld, out var playerEntity))
            {
                ref var playerComp = ref playerPool.Get(playerEntity);

                retireHeroCallback(playerComp.Hero);

                playerComp.CellIndex = -1;
                playerComp.Hero = default;

                unitPool.Del(playerEntity);
            }

            var opponentFilter = ecsWorld.Filter<OpponentComp>().End();
            var opponentPool = ecsWorld.GetPool<OpponentComp>();

            foreach (var opponentEntity in opponentFilter)
            {
                ref var opponentComp = ref opponentPool.Get(opponentEntity);

                retireHeroCallback(opponentComp.Hero);

                opponentPool.Del(opponentEntity);
                unitPool.Del(opponentEntity);
            }
        }

        private void RetireEcsRaidEnemy(LibraryRetireHero retireHeroCallback)
        {
            if (!BattleEntity.Unpack(ecsWorld, out var battleEntity))
                return;

            var battlePool = ecsWorld.GetPool<BattleComp>();
            ref var battleComp = ref battlePool.Get(battleEntity);

            if (!battleComp.EnemyPackedEntity.Unpack(ecsWorld, out var enemyEntity))
                return;

            var opponentPool = ecsWorld.GetPool<OpponentComp>();
            ref var opponentComp = ref opponentPool.Get(enemyEntity);

            retireHeroCallback(opponentComp.Hero);

            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return;

            var playerPool = ecsWorld.GetPool<PlayerComp>();
            ref var playerComp = ref playerPool.Get(playerEntity);

            playerComp.CellIndex = opponentComp.CellIndex;

            opponentPool.Del(enemyEntity);
        }

        private void TearDownEcsRaidBattle()
        {
            if (!BattleEntity.Unpack(ecsWorld, out var battleEntity))
                return;

            var battlePool = ecsWorld.GetPool<BattleComp>();
            battlePool.Del(battleEntity);
        }

        private bool TryGetPlayerUnit(
            out Unit unit,
            out int cellId)
        {
            unit = null;
            cellId = -1;

            var playerPool = ecsWorld.GetPool<PlayerComp>();
            var unitPool = ecsWorld.GetPool<UnitComp>();

            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return false;

            if (!playerPool.Has(playerEntity) ||
                !unitPool.Has(playerEntity))
                return false;

            ref var unitComp = ref unitPool.Get(playerEntity);
            unit = unitComp.Unit;

            ref var playerComp = ref playerPool.Get(playerEntity);
            cellId = playerComp.CellIndex;

            return true;
        }

        private bool TryGetPlayerHero(out Hero hero)
        {
            hero = default;
            var playerPool = ecsWorld.GetPool<PlayerComp>();

            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return false;

            if (!playerPool.Has(playerEntity))
                return false;

            ref var playerComp = ref playerPool.Get(playerEntity);
            hero = playerComp.Hero;

            return false;
        }
    }

}

