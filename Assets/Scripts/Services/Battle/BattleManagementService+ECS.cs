using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Services
{
    public partial class BattleManagementService // ECS
    {
        private EcsWorld ecsContext;

        private readonly WaitForSeconds TickTimer = new(.2f);

        private IEcsSystems ecsInitSystems;
        private IEcsSystems ecsSystems;

        public EcsPackedEntityWithWorld BattleEntity { get; internal set; } //current battle
        public EcsPackedEntityWithWorld RoundEntity { get; internal set; } // current round
        public EcsPackedEntityWithWorld TurnEntity { get; internal set; } // current turn


        private void StartEcsContext()
        {
            ecsContext = new EcsWorld();

            ecsInitSystems = new EcsSystems(ecsContext);
            ecsInitSystems
                .Add(new BattleInitSystem())
                .Add(new BattleHeroesInitSystem())
                .Inject(this)
                .Inject(libraryManager)
                .Init();

            ecsSystems = new EcsSystems(ecsContext);
            ecsSystems
                .Add(new BattlePrepareRoundSystem())
                // with DraftTag
                .Add(new BattleDraftTurnSystem())
                .Add(new BattleAssignAttackerSystem())
                .Add(new BattleAssignAttackerEffectsSystem())
                .Add(new BattleAssignTargetSystem())
                .DelHere<DraftTag>()
                // with MakeTurnTag
                .Add(new BattleApplyQueuedEffectsSystem()) // will skip next if died
                .Add(new BattleAttackSystem()) // tries to attack but can dodge/miss
                .Add(new BattleTryCastEffectsSystem()) // can pierce shield so goes 1st
                .Add(new BattleDealAttackDamageSystem())
                .Add(new BattleCompleteTurnSystem()) // summs up turn info for UI
                .Add(new BattleFinalizeTurnSystem()) // removes turn and died heroes
                .Add(new BattleDequeueDiedHeroesSystem())                 
                .Add(new GarbageCollectorSystem()) // will delete rounds and turns but not heroes

#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Inject(libraryManager)
                .Inject(prefs)
                .Init();
        }

        private void StopEcsContext()
        {
            ecsSystems?.Destroy();
            ecsSystems = null;

            ecsInitSystems?.Destroy();
            ecsInitSystems = null;

            ecsContext?.Destroy();
            ecsContext = null;
        }

        private IEnumerator BattleEcsRunloopCoroutine()
        {
            while (true)
            {
                ecsSystems.Run();
                yield return TickTimer;
            }
        }

        public HeroInstanceRefComp[] PlayerHeroes => GetEcsPlayerHeroes();
        public HeroInstanceRefComp[] EnemyHeroes => GetEcsEnemyHeroes();

        public ref Hero GetHeroConfig(EcsPackedEntityWithWorld heroInstancePackedEntity)
        {
            if (!heroInstancePackedEntity.Unpack(out var world, out var heroInstanceEntity))
                throw new Exception("No Hero entity");

            ref var configRef = ref world.GetPool<HeroConfigRefComp>().Get(heroInstanceEntity);

            if (!configRef.HeroConfigPackedEntity.Unpack(out var libWorld, out var configEntity))
                throw new Exception("No Hero config");

            ref var attackerConfig = ref libWorld.GetPool<Hero>().Get(configEntity);

            return ref attackerConfig;
        }

        private ref BattleInfo GetEcsCurrentBattle()
        {
            if (!BattleEntity.Unpack(out var world, out var entity))
                throw new Exception("No Battle");

            var battleInfoPool = world.GetPool<BattleInfo>();
            ref var battleInfo = ref battleInfoPool.Get(entity);
            return ref battleInfo;
        }

        private ref BattleRoundInfo GetEcsCurrentRound()
        {
            if (!RoundEntity.Unpack(out var world, out var entity))
                throw new Exception("No Round");

            var roundInfoPool = world.GetPool<BattleRoundInfo>();
            ref var roundInfo = ref roundInfoPool.Get(entity);

            return ref roundInfo;
        }

        private ref BattleTurnInfo GetEcsCurrentTurn()
        {
            if (!TurnEntity.Unpack(out var world, out var entity))
                throw new Exception("No Turn");

            var turnInfoPool = world.GetPool<BattleTurnInfo>();
            ref var turnInfo = ref turnInfoPool.Get(entity);
            return ref turnInfo;
        }
        private RoundSlotInfo[] GetEcsRoundSlots()
        {
            var filter = ecsContext.Filter<BattleRoundInfo>().End();
            var pool = ecsContext.GetPool<BattleRoundInfo>();

            var buffer1 = ListPool<BattleRoundInfo>.Get();
            foreach (var entity in filter)
                buffer1.Add(pool.Get(entity));

            var buffer = ListPool<RoundSlotInfo>.Get();
            foreach (var round in buffer1.OrderBy(x => x.Round))
                buffer.AddRange(round.QueuedHeroes);

            ListPool<BattleRoundInfo>.Add(buffer1);

            var retval = buffer.ToArray();

            ListPool<RoundSlotInfo>.Add(buffer);

            return retval;
        }

        private HeroInstanceRefComp[] GetEcsTaggedHeroes<T>() where T : struct
        {
            ref var battleInfo = ref GetEcsCurrentBattle();

            var filter = ecsContext.Filter<HeroInstanceRefComp>()
                .Inc<T>()
                .Exc<DeadTag>().End();

            var buffer = ListPool<HeroInstanceRefComp>.Get();

            var heroRefPool = ecsContext.GetPool<HeroInstanceRefComp>();

            foreach (var heroEntity in filter)
            {
                ref var heroRef = ref heroRefPool.Get(heroEntity);
                buffer.Add(heroRef);
            }
            var retval = buffer.ToArray();

            ListPool<HeroInstanceRefComp>.Add(buffer);

            return retval;
        }

        private HeroInstanceRefComp[] GetEcsPlayerHeroes() =>
            GetEcsTaggedHeroes<PlayerTeamTag>();

        private HeroInstanceRefComp[] GetEcsEnemyHeroes() =>
            GetEcsTaggedHeroes<EnemyTeamTag>();

        private void PrepareEcsNextTurn()
        {
            var entity = ecsContext.NewEntity();

            var battleTurnPool = ecsContext.GetPool<BattleTurnInfo>();
            ref var turnInfo = ref battleTurnPool.Add(entity);

            var draftTagPool = ecsContext.GetPool<DraftTag>();
            draftTagPool.Add(entity);

            TurnEntity = ecsContext.PackEntityWithWorld(entity);
        }

        private void MakeEcsTurn()
        {
            if (TurnEntity.Unpack(out var world, out var entity))
                throw new Exception("No turn");

            ecsContext.GetPool<MakeTurnTag>().Add(entity);
        }

        private void SetEcsTurnProcessed()
        {
            if (TurnEntity.Unpack(out var world, out var entity))
                throw new Exception("No turn");

            ecsContext.GetPool<ProcessedTurnTag>().Add(entity);
        }
    }
}