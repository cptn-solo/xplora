using Leopotam.EcsLite;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
//using Google.Apis.Sheets.v4.Data;

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
            ecsInitSystems = new EcsSystems(ecsWorld);
            ecsRunSystems = new EcsSystems(ecsWorld);

            ecsInitSystems
                .Add(new RaidInitSystem())
                .Add(new OpponentInitSystem())
                .Add(new PlayerInitSystem())
                .Inject(this)
                .Inject(worldService)
                .Init();

            ecsRunSystems
                .Add(new PlayerTeamCardsSpawnSystem())
                .Add(new PlayerTeamCardsUnlinkSystem()) // unlink despawned cards
                .Add(new PlayerTeamMemberRetireSystem())
                .Add(new PlayerTeamUpdateSystem()) // bufs
                .Add(new OpponentPositionSystem())
                .Add(new PlayerPositionSystem())
                .Add(new PlayerTeamUpdateHPSystem()) //HP update on team cards
                .Add(new PlayerTeamUpdateBufSystem<DamageRangeComp>()) //Buf icons update on team cards
                                                                       //.DelHere<UpdateHPTag>()
                .Add(new OutOfPowerSystem())
                // with BattleAftermathComp:
                .Add(new ProcessTeamMemberDeath())
                .Add(new BattleAftermathSystem())
                .Add(new BattleTrophyCounterSystem())
                .Add(new RaidBalanceUpdateSystem())
                .Add(new PlayerTeamUpdateDebufSystem<DamageRangeComp>()) //remove buff and icons update on team cards
                .DelHere<BattleAftermathComp>()
                .Add(new RemoveWorldPoiSystem())
                .Add(new RaidTeardownSystem())
                .Add(new RetireEnemySystem())
                .Add(new RetirePlayerSystem())
                .Add(new MoveToCellSystem())
                .DelHere<VisitCellComp>()
                .Add(new DeployUnitSystem())
                .Add(new DeployUnitOverlaySystem())
                .DelHere<ProduceTag>()
                .Add(new VisitPowerSourceSystem())
                .Add(new VisitTerrainAttributeSystem())
                .DelHere<VisitedComp<OpponentComp>>()
                .DelHere<VisitedComp<PowerSourceComp>>()
                .DelHere<VisitedComp<WatchTowerComp>>()
                .DelHere<VisitedComp<TerrainAttributeComp>>()
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
            var unitRefPool = ecsWorld.GetPool<UnitRef>();

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
            opponentHeroes = libManagementService.NonPlayerTeamHeroes;

            return playerHeroes.Length > 0;
        }

        private void DeployEcsWorldUnits(int cellIndex)
        {
            State = Data.RaidState.AwaitingUnits;

            var producePool = ecsWorld.GetPool<ProduceTag>();
            var unitRefPool = ecsWorld.GetPool<UnitRef>();

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

            ref var heroConfigRef = ref world.GetPool<HeroConfigRefComp>().Get(entity);

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

            if (!heroComp.Packed.Unpack(out var libWorld, out var libEntity))
                throw new Exception("No Hero config");

            enemyHero = libWorld.GetPool<Hero>().Get(libEntity);

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
            if (RaidEntity.Unpack(ecsWorld, out var raidEntity))
                ecsWorld.DelEntity(raidEntity);
        }

        private void BoostEcsTeamMemberSpecOption(
            EcsPackedEntityWithWorld heroEntity, SpecOption specOption, int factor)
        {
            libManagementService.BoostSpecOption(
                currentEventInfo.Value.EventHero,
                specOption,
                factor);

            if (!heroEntity.Unpack(out var world, out var entity))
                throw new Exception("No Hero instance");

            var pool = world.GetPool<UpdateTag>();
            if (!pool.Has(entity))
                pool.Add(entity);
        }

        private void BoostEcsNextBattleSpecOption(
            EcsPackedEntityWithWorld heroEntity, SpecOption specOption, int factor)
        {
            // TODO: Add Spec Option Boost for the next battle
            //1. pick player team hero for eventHero
            if (!RaidEntity.Unpack(ecsWorld, out var raidEntity))
                throw new Exception("No Raid");

            if (!heroEntity.Unpack(out var world, out var entity))
                throw new Exception("No Hero instance");

            switch (specOption)
            {
                case SpecOption.DamageRange:
                    {
                        var updatePool = world.GetPool<UpdateBuffsTag<DamageRangeComp>>();
                        //buff*=2
                        var pool = world.GetPool<BuffComp<DamageRangeComp>>();
                        if (!pool.Has(entity))
                            pool.Add(entity);

                        ref var buff = ref pool.Get(entity);
                        buff.Value += ((buff.Value == 0 ? 100 : 0) + factor);
                        buff.Icon = BundleIcon.ShieldCrossed;
                        buff.IconColor = Color.cyan;

                        if (!updatePool.Has(entity))
                            updatePool.Add(entity);
                    }
                    break;
                case SpecOption.Health:
                    {
                        var updatePool = world.GetPool<UpdateHPTag>();
                        //hp = max(health, hp*=2)
                        var healthPool = world.GetPool<HealthComp>();
                        ref var healthComp = ref healthPool.Get(entity);

                        var hpPool = world.GetPool<HPComp>();
                        ref var hpComp = ref hpPool.Get(entity);

                        hpComp.Value = Mathf.Min(healthComp.Value, hpComp.Value * 2);

                        if (!updatePool.Has(entity))
                            updatePool.Add(entity);
                    }
                    break;
                case SpecOption.UnlimitedStaminaTag:
                    {
                        var pool = world.GetPool<BuffComp<NoStaminaDrainBuffTag>>();
                        if (!PlayerEntity.Unpack(out _, out var playerEntity))
                            throw new Exception("No Player entity");

                        if (!pool.Has(playerEntity))
                            pool.Add(playerEntity);
                    }
                    break;
                case SpecOption.DefenceRate:
                case SpecOption.AccuracyRate:
                case SpecOption.DodgeRate:
                case SpecOption.Speed:
                default:
                    break;
            }

            //2. add boostComponent of specOption to the raid hero entitity instance
            // 

        }



    }
}

