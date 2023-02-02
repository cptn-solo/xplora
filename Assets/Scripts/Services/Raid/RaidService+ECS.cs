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
                .Add(new BattleAftermathSystem())
                .Add(new RaidTeardownSystem())
                .Add(new DeployUnitSystem())
                .Add(new BattleLaunchSystem())
                .Add(new DestroyUnitSystem())
                .Add(new RetireEnemySystem())
                .Add(new RetirePlayerSystem())
                .Add(new RemoveWorldPoiSystem())
                .Add(new VisitSystem())
                .Add(new LeaveSystem())
                .Add(new RefillSystem())
                .Add(new DrainSystem())
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

        internal void StopEcsRaidContext()
        {
            runLoopActive = false;

            if (raidRunloopCoroutine != null)
                StopCoroutine(raidRunloopCoroutine);

            raidRunloopCoroutine = null;

            if (ecsRaidContext != null)
                DestroyEcsRaidContext();
        }

        private void DeployEcsWorldUnits()
        {
            var cellFilter = ecsRaidContext
                .Filter<FieldCellComp>().Inc<HeroComp>().End();

            var producePool = ecsRaidContext.GetPool<ProduceTag>();

            foreach (var entity in cellFilter)
                producePool.Add(entity);
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

            var cellPool = ecsRaidContext.GetPool<FieldCellComp>();
            ref var cellComp = ref cellPool.Get(playerEntity);
            cellComp.CellIndex = cellId;
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

