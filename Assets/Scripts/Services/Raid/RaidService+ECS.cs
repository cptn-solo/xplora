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
        private EcsWorld ecsRaidContext = null;
        private IEcsSystems ecsRunSystems = null;
        private IEcsSystems ecsInitSystems = null;

        public EcsPackedEntity PlayerEntity { get; set; }
        public EcsPackedEntity WorldEntity { get; set; }
        public EcsPackedEntity RaidEntity { get; set; }
        public EcsPackedEntity? BattleEntity { get; set; } = null;

        private bool GetActiveTeamMemberForTrait(HeroTrait trait,
            out Hero? eventHero,
            out EcsPackedEntityWithWorld? eventHeroEntity,
            out int maxLevel)
        {

            eventHero = null;
            eventHeroEntity = null;
            maxLevel = 0;

            if (!RaidEntity.Unpack(ecsRaidContext, out var raidEntity))
                return false;

            var filter = ecsRaidContext.Filter<PlayerTeamTag>().Inc<HeroConfigRefComp>().End();
            var heroConfigRefPool = ecsRaidContext.GetPool<HeroConfigRefComp>();

            var buffer = ListPool<EcsPackedEntityWithWorld>.Get();
            var maxedHeroes = ListPool<Hero>.Get();

            foreach (var entity in filter)
            {
                ref var heroConfigRef = ref heroConfigRefPool.Get(entity);
                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero config");

                ref var hero = ref libWorld.GetPool<Hero>().Get(libEntity);

                if (hero.Traits.TryGetValue(trait, out var traitInfo) &&
                    traitInfo.Level > 0 &&
                    traitInfo.Level >= maxLevel)
                {
                    maxLevel = traitInfo.Level;
                    maxedHeroes.Add(hero);
                    buffer.Add(ecsRaidContext.PackEntityWithWorld(entity));
                }
            }
            if (maxedHeroes.Count > 0)
            {
                var idx = Random.Range(0, maxedHeroes.Count);
                eventHero = maxedHeroes[idx];
                eventHeroEntity = buffer[idx];
            }

            ListPool<Hero>.Add(maxedHeroes);

            var retval = buffer.Count > 0;

            ListPool<EcsPackedEntityWithWorld>.Add(buffer);

            return retval;
        }
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

                .Add(new PlayerTeamUpdateSystem()) // bufs
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
                .Inject(battleManagementService)
                .Inject(libManagementService)
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

        private void ProcessEcsDeathInBattle()
        {            
            if (!RaidEntity.Unpack(ecsRaidContext, out var raidEntity))
                return;

            if (!PlayerEntity.Unpack(ecsRaidContext, out var playerEntity))
                return;

            var heroPool = ecsRaidContext.GetPool<HeroComp>();
            var filter = ecsRaidContext.Filter<PlayerTeamTag>().Inc<HeroConfigRefComp>().End();

            if (filter.GetEntitiesCount() == 0)
            {
                heroPool.Del(playerEntity);
                return;
            }

            var heroBuffer = ListPool<Hero>.Get();
            var heroPackedBuffer = ListPool<EcsPackedEntityWithWorld>.Get();

            foreach (var heroInstanceEntity in filter)
            {
                var heroConfigRefPool = ecsRaidContext.GetPool<HeroConfigRefComp>();
                ref var heroConfigRef = ref heroConfigRefPool.Get(heroInstanceEntity);

                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero Config");

                var heroConfigPool = libWorld.GetPool<Hero>();
                ref var heroConfig = ref heroConfigPool.Get(libEntity);
                heroBuffer.Add(heroConfig);
                heroPackedBuffer.Add(heroConfigRef.Packed);
            }
            var bestSpeed = heroBuffer.ToArray().HeroBestBySpeed(out var idx);
            var bestSpeedPacked = heroPackedBuffer[idx];

            if (!heroPool.Has(playerEntity))
                heroPool.Add(playerEntity);

            ref var heroComp = ref heroPool.Get(playerEntity);

            heroComp.Hero = bestSpeedPacked;

            ListPool<EcsPackedEntityWithWorld>.Add(heroPackedBuffer);
            ListPool<Hero>.Add(heroBuffer);

        }

        private void ProcessEcsBattleAftermath(bool won)
        {
            Debug.Log($"ProcessEcsBattleAftermath {won}");

            if (BattleEntity == null || !BattleEntity.Value.Unpack(ecsRaidContext, out var battleEntity))
            {
                menuNavigationService.NavigateToScreen(Screens.HeroesLibrary);
                return;
            }

            ProcessEcsDeathInBattle();

            var aftermathPool = ecsRaidContext.GetPool<BattleAftermathComp>();
            ref var aftermathComp = ref aftermathPool.Add(battleEntity);
            aftermathComp.Won = won;

            BattleEntity = null;
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

            if (!visitPool.Has(playerEntity))
                visitPool.Add(playerEntity);

            ref var visitComp = ref visitPool.Get(playerEntity);
            visitComp.CellIndex = cellId;
        }

        private bool CheckEcsRaidForBattle()
        {
            if (BattleEntity != null &&
                BattleEntity.Value.Unpack(ecsRaidContext, out var battleEntity) &&
                ecsRaidContext.GetPool<BattleComp>().Has(battleEntity))
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
                sourceWorld != ecsRaidContext)
                return false;
            
            enemyEntity = ecsRaidContext.PackEntity(entity);

            var heroPool = sourceWorld.GetPool<HeroComp>();
            ref var heroComp = ref heroPool.Get(entity);

            if (!heroComp.Packed.Unpack(out var libWorld, out var libEntity))
                throw new Exception("No Hero config");

            enemyHero = libWorld.GetPool<Hero>().Get(libEntity);

            return true;
        }

        private bool CheckEcsWorldForAttributes(
            int cellId,
            out TerrainAttribute attribute)
        {
            attribute = TerrainAttribute.NA;

            if (!worldService.TryGetAttribute(cellId, out attribute))
                return false;

            return true;
        }
        private void TryCastEcsTerrainEvent(TerrainEventConfig eventConfig,
            Hero eventHero, EcsPackedEntityWithWorld eventHeroEntity, int maxLevel)
        {
            if (!(5 + (maxLevel * 5)).RatedRandomBool())
            {
                Debug.Log($"Missed Event: {eventConfig}");
                return;
            }

            currentEventInfo = WorldEventInfo.Create(eventConfig,
                eventHero, eventHeroEntity, maxLevel);

            dialog.SetEventInfo(currentEventInfo.Value);
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
            if (!RaidEntity.Unpack(ecsRaidContext, out var raidEntity))
                throw new Exception("No Raid");

            if (!heroEntity.Unpack(out var world, out var entity))
                throw new Exception("No Hero instance");

            switch (specOption)
            {
                case SpecOption.DamageRange:
                    {
                        //buff*=2
                        var pool = world.GetPool<BuffComp<DamageRangeComp>>();
                        if (!pool.Has(entity))
                            pool.Add(entity);

                        ref var buff = ref pool.Get(entity);
                        buff.Value += factor;
                    }
                    break;
                case SpecOption.Health:
                    {

                        //hp = max(health, hp*=2)
                        var healthPool = world.GetPool<HealthComp>();
                        ref var healthComp = ref healthPool.Get(entity);

                        var hpPool = world.GetPool<HPComp>();
                        ref var hpComp = ref hpPool.Get(entity);

                        hpComp.Value = Mathf.Min(healthComp.Value, hpComp.Value * 2);
                    
                    }
                    break;
                case SpecOption.DefenceRate:
                case SpecOption.AccuracyRate:
                case SpecOption.DodgeRate:
                case SpecOption.Speed:
                case SpecOption.UnlimitedStaminaTag:
                default:
                    break;
            }

            //2. add boostComponent of specOption to the raid hero entitity instance
            // 

        }



    }
}

