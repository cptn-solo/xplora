using Leopotam.EcsLite;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;
using System;

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

        private Hero[] GetActiveTeamMembers()
        {
            if (!RaidEntity.Unpack(ecsRaidContext, out var raidEntity))
                return new Hero[0];

            var filter = ecsRaidContext.Filter<PlayerTeamTag>().Inc<HeroConfigRefComp>().End();
            var heroConfigRefPool = ecsRaidContext.GetPool<HeroConfigRefComp>();
            var buffer = ListPool<Hero>.Get();

            foreach (var entity in filter)
            {
                ref var heroConfigRef = ref heroConfigRefPool.Get(entity);
                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero config");

                buffer.Add(libWorld.GetPool<Hero>().Get(libEntity));
            }
            var retval = buffer.ToArray();

            ListPool<Hero>.Add(buffer);

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
            if (!BattleEntity.Unpack(ecsRaidContext, out var battleEntity))
                return;

            if (!RaidEntity.Unpack(ecsRaidContext, out var raidEntity))
                return;


            if (!PlayerEntity.Unpack(ecsRaidContext, out var playerEntity))
                return;

            var heroPool = ecsRaidContext.GetPool<HeroComp>();
            ref var heroComp = ref heroPool.Get(playerEntity);

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

            heroComp.Hero = bestSpeedPacked;

            ListPool<EcsPackedEntityWithWorld>.Add(heroPackedBuffer);
            ListPool<Hero>.Add(heroBuffer);

        }

        private void ProcessEcsBattleAftermath(bool won)
        {
            if (!BattleEntity.Unpack(ecsRaidContext, out var battleEntity))
                return;

            ProcessEcsDeathInBattle();

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

        private bool CheckEcsRaidForBattle()
        {
            if (BattleEntity.Unpack(ecsRaidContext, out var battleEntity) &&
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

        private void BoostEcsNextBattleSpecOption(Hero eventHero, SpecOption specOption, int factor)
        {
            //TODO: Add Spec Option Boost for the next battle
            //1. pick player team hero for eventHero
            if (!RaidEntity.Unpack(ecsRaidContext, out var raidEntity))
                throw new Exception("No Raid");

            var raidInfoPool = ecsRaidContext.GetPool<RaidComp>();

            //2. add boostComponent of specOption to the raid hero entitity instance
            // 

        }



    }
}

