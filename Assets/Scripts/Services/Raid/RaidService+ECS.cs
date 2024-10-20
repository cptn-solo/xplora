﻿using Leopotam.EcsLite;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite.Di;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;
using UnityEngine;
using Assets.Scripts.ECS;

namespace Assets.Scripts.Services
{
    public partial class RaidService // ECS
    {
        public EcsPackedEntityWithWorld PlayerEntity { get; set; }
        public EcsPackedEntity WorldEntity { get; set; }
        public EcsPackedEntity RaidEntity { get; set; }
        public EcsPackedEntity? BattleEntity { get; set; } = null;
        
        /// <summary>
        /// Run before new raid, not after return from the battle/event
        /// </summary>
        private void InitEcsWorld()
        {
            ecsWorld = new EcsWorld();
            ecsInitSystems = new EcsSystems(ecsWorld, new SharedEcsContext());
            ecsRunSystems = new EcsSystems(ecsWorld, new SharedEcsContext());

            ecsInitSystems
                .Add(new RaidInitSystem())
                .Add(new RaidUIInitSystem())                
                .Add(new OpponentInitSystem())
                .Add(new PlayerInitSystem())
                .Add(new PlayerTeamMemberInitSystem())
                .Add(new PlayerTeamMemberTraitsInitSystem())
                .Add(new PlayerTeamMemberRelationsInitSystem())
                .Add(new PlayerTeamRelationPartiesInitSystem())
                .Add(new PlayerTeamRelationScoresInitSystem())
                .Inject(this)
                .Inject(worldService)
                .Inject(libManagementService)
                .Init();

            ecsRunSystems
                .Add(new PlayerTeamCardsSpawnSystem())
                .Add(new PlayerTeamMemberRetireSystem())
                .Add(new PlayerTeamUpdateSystem()) // bufs #1 (not sure why we need it here, assume for init only)
                .Add(new OpponentPositionSystem())
                .Add(new PlayerPositionSystem())
                .Add(new PlayerTeamUpdateHPSystem()) //HP update on team cards
                .Add(new PlayerTeamUpdateBarsInfoSystem())
                .Add(new PlayerTeamUpdateBufSystem<IntRangeValueComp<DamageRangeTag>>()) //Buf icons update on team cards
                .Add(new PlayerTeamUpdateHoverSystem())
                .Add(new PlayerTeamUpdateScoreSystem())
                .Add(new OutOfPowerSystem())
                // with BattleAftermathComp:
                .Add(new ProcessTeamMemberDeath())
                .Add(new BattleAftermathSystem())
                .Add(new BattleTrophyCounterSystem())
                .Add(new RaidBalanceUpdateSystem())
                .Add(new PlayerTeamUpdateDebufSystem<IntRangeValueComp<DamageRangeTag>>()) //remove buff and icons update on team cards
                .CleanupHere<BattleAftermathComp>()
                .Add(new RemoveWorldPoiSystem())
                .Add(new RaidTeardownSystem())
                .Add(new RetireEnemySystem())
                .Add(new RetirePlayerSystem())
                .Add(new MoveToCellSystem())
                .Add(new RelationsEventTriggerSystem())
                .CleanupHere<VisitCellComp>()
                .Add(new DeployUnitSystem())
                .Add(new DeployUnitOverlaySystem())
                .CleanupHere<ProduceTag>()
                .Add(new UpdateUnitStrengthSystem())
                .Add(new PickTeamMemberForWatchTowerEvent())
                .Add(new PickTeamMemberForTerrainAttributeEvent())
                .Add(new VisitPowerSourceSystem())
                .Add(new VisitHPSourceSystem())
                .Add(new VisitWatchTowerSystem())
                .Add(new VisitTerrainAttributeSystem())
                .Add(new RelationsEventSystem())
                .Add(new PlayerTeamKindsPanelToggleSystem())
                .Add(new PlayerTeamTriggerKindsPanelUpdateSystem())
                .Add(new PlayerTeamResetKindsPanelSystem())
                .Add(new PlayerTeamUpdateKindsPanelSystem())
                .CleanupHere<VisitedComp<OpponentComp>>()
                .CleanupHere<VisitedComp<HPSourceComp>>()
                .CleanupHere<VisitedComp<PowerSourceComp>>()
                .CleanupHere<VisitedComp<WatchTowerComp>>()
                .CleanupHere<VisitedComp<TerrainAttributeComp>>()
                .Add(new ShowDialogSystem<WorldEventInfo>())
                .Add(new ShowDialogSystem<RelationsEventInfo>())
                .Add(new ShowToastSystem<RelationsEventToastInfo>())
                .Add(new AutoDismissToastSystem<RelationsEventToastInfo>())
                .Add(new ProcessWorldEventAction())
                .Add(new PlayerTeamUpdateSystem()) // bufs #2
                .Add(new ProcessRelationsEventAction())
                .Add(new RefillSystem())
                .CleanupHere<RefillComp>()
                .Add(new DrainSystem())
                .CleanupHere<DrainComp>()
                .Add(new UpdateUnitOverlaySystem())
                .CleanupHere<UpdateTag>()
                .Add(new BattleLaunchSystem())
                .CleanupHere<DraftTag>()
                .Add(new DestroyUnitOverlaySystem())
                .Add(new DestroyUnitSystem())
                .CleanupHere<DestroyTag>()
                .Add(new GarbageCollectorSystem())
                .Add(new RaidTerminationSystem())
#if UNITY_EDITOR
                .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Inject(worldService)
                .Inject(battleManagementService)
                .Inject(libManagementService)
                .Inject(audioService)
                .Init();

        }
        /// <summary>
        /// Run on raid completion (return to the library after raid was lost)
        /// </summary>
        private void DestroyecsWorld()
        {
            ecsRunSystems?.Destroy();
            ecsRunSystems = null;

            ecsInitSystems?.Destroy();
            ecsInitSystems = null;

            ecsWorld?.Destroy();
            ecsWorld = null;
        }

        private void StartEcsWorld()
        {
            if (ecsWorld == null)
                InitEcsWorld();

            runloopCoroutine ??= StartCoroutine(RunloopCoroutine());
        }

        internal void StopEcsWorld()
        {
            StopRunloopCoroutine();

            if (ecsWorld != null)
                DestroyecsWorld();

            menuNavigationService.NavigateToScreen(Screens.HeroesLibrary);
        }

        private void DestroyEcsWorldUnits(int cellIndex)
        {
            var destroyPool = ecsWorld.GetPool<DestroyTag>();
            var unitRefPool = ecsWorld.GetPool<EntityViewRef<Hero>>();

            if (worldService.TryGetPoi<OpponentComp>(cellIndex, out var opponentPackedEntity) &&
                opponentPackedEntity.Unpack(out var world, out var opponentEntity) &&
                world == ecsWorld)
            {
                if (unitRefPool.Has(opponentEntity) &&
                    !destroyPool.Has(opponentEntity))
                    destroyPool.Add(opponentEntity);
            }
        }

        /// <summary>
        /// Called from ecs during initialization
        /// </summary>
        /// <param name="playerHeroes"></param>
        /// <param name="opponentHeroes"></param>
        /// <returns></returns>
        public bool AssignPlayerAndEnemies(
            out EcsPackedEntityWithWorld[] playerHeroes,
            out EcsPackedEntityWithWorld[] opponentHeroes)
        {
            playerHeroes = libManagementService.PlayerHeroes;
            opponentHeroes = libManagementService.EnemyDomainHeroes;

            return playerHeroes.Length > 0;
        }

        private void DeployEcsWorldUnits(int cellIndex)
        {
            State = RaidState.AwaitingUnits;

            var producePool = ecsWorld.GetPool<ProduceTag>();
            var unitRefPool = ecsWorld.GetPool<EntityViewRef<Hero>>();

            var cellPool = ecsWorld.GetPool<FieldCellComp>();
            if (PlayerEntity.Unpack(out _, out var playerEntity))
            {
                ref var cellComp = ref cellPool.Get(playerEntity);
                if (cellComp.CellIndex == cellIndex &&
                    !unitRefPool.Has(playerEntity) &&
                    !producePool.Has(playerEntity))
                    producePool.Add(playerEntity);
            }

            if (worldService.TryGetPoi<OpponentComp>(cellIndex, out var opponentPackedEntity) &&
                opponentPackedEntity.Unpack(out var world, out var opponentEntity) &&
                world == ecsWorld)
            {
                if (!unitRefPool.Has(opponentEntity) &&
                    !producePool.Has(opponentEntity))
                    producePool.Add(opponentEntity);
            }
        }
        public TeamMemberInfo GetTeamMemberInfoForPackedEntity(EcsPackedEntityWithWorld? packed)
        {
            if (packed == null || !packed.Value.Unpack(out var world, out var entity))
                return default;

            ref var heroConfigRef = ref world.GetPool<HeroConfigRef>().Get(entity);

            if (!heroConfigRef.HeroConfigPackedEntity.Unpack(out var libWorld, out var configEntity))
                return default;

            ref var heroConfig = ref libWorld.GetPool<Hero>().Get(configEntity);

            var retval = new TeamMemberInfo()
            {
                HeroName = heroConfig.Name,
                IconName = heroConfig.IconName,
                IdleSpriteName = heroConfig.IdleSpriteName,
                Speed = heroConfig.Speed // probably redundant
            };
            return retval;
        }
        
        private void ProcessEcsBattleAftermath(bool won, Asset[] pot)
        {
            Debug.Log($"ProcessEcsBattleAftermath {won}");

            if (BattleEntity == null || !BattleEntity.Value.Unpack(ecsWorld, out var battleEntity))
            {
                menuNavigationService.NavigateToScreen(Screens.HeroesLibrary);
                return;
            }

            var aftermathPool = ecsWorld.GetPool<BattleAftermathComp>();
            ref var aftermathComp = ref aftermathPool.Add(battleEntity);
            aftermathComp.Won = won;
            aftermathComp.Trophy = pot;

            BattleEntity = null;
        }

        private void VisitEcsCellId(int cellId = -1)
        {
            if (!PlayerEntity.Unpack(out _, out var playerEntity))
                return;

            if (cellId == -1) // 1st appearance in the world after lib/battle
            {
                var cellPool = ecsWorld.GetPool<FieldCellComp>();
                ref var cellComp = ref cellPool.Get(playerEntity);
                cellId = cellComp.CellIndex;
            }

            var visitPool = ecsWorld.GetPool<VisitCellComp>();

            if (!visitPool.Has(playerEntity))
                visitPool.Add(playerEntity);

            ref var visitComp = ref visitPool.Get(playerEntity);
            visitComp.CellIndex = cellId;
        }

        private bool CheckEcsRaidForBattle()
        {
            if (BattleEntity != null &&
                BattleEntity.Value.Unpack(ecsWorld, out var battleEntity) &&
                ecsWorld.GetPool<BattleComp>().Has(battleEntity))
                return true;

            return false;
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
                sourceWorld != ecsWorld)
                return false;
            
            enemyEntity = ecsWorld.PackEntity(entity);

            var heroPool = sourceWorld.GetPool<HeroComp>();
            ref var heroComp = ref heroPool.Get(entity);

            enemyHero = libManagementService.GetHeroConfigForLibraryHeroInstance(heroComp.Packed);

            return true;
        }

        private void InitiateEcsWorldBattle(EcsPackedEntity enemyPackedEntity)
        {
            if (!enemyPackedEntity.Unpack(ecsWorld, out var enemyEntity))
                return;

            var battlePool = ecsWorld.GetPool<BattleComp>();
            var draftPool = ecsWorld.GetPool<DraftTag>();
            var battleEntity = ecsWorld.NewEntity();

            ref var battleComp = ref battlePool.Add(battleEntity);
            battleComp.EnemyPackedEntity = ecsWorld.PackEntity(enemyEntity);

            draftPool.Add(battleEntity);

            BattleEntity = ecsWorld.PackEntity(battleEntity);
        }

        private void MarkEcsWorldRaidForTeardown()
        {
            if (ecsWorld != null && RaidEntity.Unpack(ecsWorld, out var raidEntity))
                ecsWorld.DelEntity(raidEntity);
        }        

    }
}

