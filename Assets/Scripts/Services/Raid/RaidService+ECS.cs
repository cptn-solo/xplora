using Leopotam.EcsLite;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;
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
        private IEcsSystems ecsRunSystems = null;
        private IEcsSystems ecsInitSystems = null;

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
            ecsInitSystems = new EcsSystems(ecsRaidContext);
            ecsRunSystems = new EcsSystems(ecsRaidContext);

            ecsInitSystems
                .Add(new RaidInitSystem())
                .Add(new OpponentInitSystem())
                .Add(new PlayerInitSystem())
                .Inject(this)
                .Inject(worldService)
                .Init();

            ecsRunSystems
                .Add(new OpponentPositionSystem())
                .Add(new PlayerPositionSystem())
                .Add(new OutOfPowerSystem())
                .Add(new BattleAftermathSystem())
                .Add(new RemoveWorldPoiSystem())
                .Add(new RaidTeardownSystem())
                .Add(new RetireEnemySystem())
                .Add(new RetirePlayerSystem())
                .Add(new MoveSightSystem())
                .Add(new DeployUnitSystem())
                .Add(new DeployUnitOverlaySystem())
                .DelHere<ProduceTag>()
                .Add(new VisitSystem())
                .DelHere<VisitCellComp>()
                .Add(new RefillSystem())
                .DelHere<RefillComp>()
                .Add(new DrainSystem())
                .DelHere<DrainComp>()
                .Add(new UpdateUnitOverlaySystem())
                .DelHere<UpdateTag>()
                .Add(new BattleLaunchSystem())
                .DelHere<DraftTag>()
                .Add(new DestroyUnitOverlaySystem())
                .Add(new DestroyUnitSystem())
                .DelHere<DestroyTag>()
                .Add(new GarbageCollectorSystem())
                .Add(new RaidTerminationSystem())
#if UNITY_EDITOR
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Inject(worldService)
                .Init();

        }
        /// <summary>
        /// Run on raid completion (return to the library after raid was lost)
        /// </summary>
        private void DestroyEcsRaidContext()
        {
            ecsRunSystems?.Destroy();
            ecsRunSystems = null;

            ecsInitSystems?.Destroy();
            ecsInitSystems = null;

            ecsRaidContext?.Destroy();
            ecsRaidContext = null;
        }

        private void StartEcsRaidContext()
        {
            if (ecsRaidContext == null)
                InitEcsRaidContext();

            raidRunloopCoroutine ??= StartCoroutine(RaidRunloopCoroutine());
        }

        internal void StopEcsRaidContext()
        {
            runLoopActive = false;

            if (raidRunloopCoroutine != null)
                StopCoroutine(raidRunloopCoroutine);

            raidRunloopCoroutine = null;

            if (ecsRaidContext != null)
                DestroyEcsRaidContext();
        }

        private void DestroyEcsWorldUnits(int cellIndex)
        {
            var destroyPool = ecsRaidContext.GetPool<DestroyTag>();
            var unitRefPool = ecsRaidContext.GetPool<UnitRef>();

            if (worldService.TryGetPoi<OpponentComp>(cellIndex, out var opponentPackedEntity) &&
                opponentPackedEntity.Unpack(out var world, out var opponentEntity) &&
                world == ecsRaidContext)
            {
                if (unitRefPool.Has(opponentEntity) &&
                    !destroyPool.Has(opponentEntity))
                    destroyPool.Add(opponentEntity);
            }

        }
        private void DeployEcsWorldUnits(int cellIndex)
        {
            State = Data.RaidState.AwaitingUnits;

            var producePool = ecsRaidContext.GetPool<ProduceTag>();
            var unitRefPool = ecsRaidContext.GetPool<UnitRef>();

            var cellPool = ecsRaidContext.GetPool<FieldCellComp>();
            if (PlayerEntity.Unpack(ecsRaidContext, out var playerEntity))
            {
                ref var cellComp = ref cellPool.Get(playerEntity);
                if (cellComp.CellIndex == cellIndex &&
                    !unitRefPool.Has(playerEntity) &&
                    !producePool.Has(playerEntity))
                    producePool.Add(playerEntity);
            }

            if (worldService.TryGetPoi<OpponentComp>(cellIndex, out var opponentPackedEntity) &&
                opponentPackedEntity.Unpack(out var world, out var opponentEntity) &&
                world == ecsRaidContext)
            {
                if (!unitRefPool.Has(opponentEntity) &&
                    !producePool.Has(opponentEntity))
                    producePool.Add(opponentEntity);
            }
        }


        private void ProcessEcsBattleAftermath(bool won)
        {
            if (!BattleEntity.Unpack(ecsRaidContext, out var battleEntity))
                return;

            var aftermathPool = ecsRaidContext.GetPool<BattleAftermathComp>();
            ref var aftermathComp = ref aftermathPool.Add(battleEntity);
            aftermathComp.Won = won;
        }

        private void VisitEcsCellId(int cellId = -1)
        {
            if (!PlayerEntity.Unpack(ecsRaidContext, out var playerEntity))
                return;

            if (cellId == -1) // 1st appearance in the world after lib/battle
            {
                var cellPool = ecsRaidContext.GetPool<FieldCellComp>();
                ref var cellComp = ref cellPool.Get(playerEntity);
                cellId = cellComp.CellIndex;
            }

            var visitPool = ecsRaidContext.GetPool<VisitCellComp>();
            ref var visitComp = ref visitPool.Add(playerEntity);
            visitComp.CellIndex = cellId;
        }

        private bool CheckEcsWorldForOpponent(
            int cellId,
            out Hero enemyHero,
            out EcsPackedEntity enemyEntity)
        {
            enemyEntity = default;
            enemyHero = default;

            if (!worldService.TryGetPoi<OpponentComp>(cellId, out EcsPackedEntityWithWorld packedEntity))
                return false;

            if (!packedEntity.Unpack(out var sourceWorld, out var entity) ||
                sourceWorld != ecsRaidContext)
                return false;
            
            enemyEntity = ecsRaidContext.PackEntity(entity);

            var heroPool = sourceWorld.GetPool<HeroComp>();
            ref var heroComp = ref heroPool.Get(entity);
            enemyHero = heroComp.Hero;

            return true;
        }

        private void InitiateEcsWorldBattle(EcsPackedEntity enemyPackedEntity)
        {
            if (!enemyPackedEntity.Unpack(ecsRaidContext, out var enemyEntity))
                return;

            var battlePool = ecsRaidContext.GetPool<BattleComp>();
            var draftPool = ecsRaidContext.GetPool<DraftTag>();
            var battleEntity = ecsRaidContext.NewEntity();

            ref var battleComp = ref battlePool.Add(battleEntity);
            battleComp.EnemyPackedEntity = ecsRaidContext.PackEntity(enemyEntity);

            draftPool.Add(battleEntity);

            BattleEntity = ecsRaidContext.PackEntity(battleEntity);
        }

        private void MarkEcsWorldRaidForTeardown()
        {
            if (RaidEntity.Unpack(ecsRaidContext, out var raidEntity))
                ecsRaidContext.DelEntity(raidEntity);
        }


    }
}

