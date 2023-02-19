using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
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
                .Add(new BattleCleanupCompletedRoundSystem())
                .Add(new BattlePrepareRoundSystem())
                .Add(new BattleDraftTurnSystem())
                .Add(new BattlePrepareTurnSystem())
                .Add(new BattleApplyQueuedEffectsSystem())
                .Add(new BattleAttackSystem())
                .Add(new BattleTryCastEffectsSystem())
                .Add(new BattleCompleteTurnSystem())
                .Add(new GarbageCollectorSystem())

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

        private HeroInstanceRefComp[] GetEcsHeroes(int teamId)
        {
            ref var battleInfo = ref GetEcsCurrentBattle();

            var filter = teamId == battleInfo.PlayerTeam.Id ?
                ecsContext.Filter<HeroInstanceRefComp>().Inc<PlayerTeamTag>().Exc<DeadTag>().End() :
                ecsContext.Filter<HeroInstanceRefComp>().Inc<EnemyTeamTag>().Exc<DeadTag>().End();

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

        private HeroInstanceRefComp[] GetEcsPlayerHeroes()
        {
            ref var battleInfo = ref GetEcsCurrentBattle();
            return GetEcsHeroes(battleInfo.PlayerTeam.Id);
        }

        private HeroInstanceRefComp[] GetEcsEnemyHeroes()
        {
            ref var battleInfo = ref GetEcsCurrentBattle();
            return GetEcsHeroes(battleInfo.EnemyTeam.Id);
        }

        private void PrepareEcsNextTurn()
        {
            var entity = ecsContext.NewEntity();

            var draftTagPool = ecsContext.GetPool<DraftTag>();
            draftTagPool.Add(entity);

            var battleTurnPool = ecsContext.GetPool<BattleTurnInfo>();
            ref var turnInfo = ref battleTurnPool.Add(entity);
        }

        private void MakeEcsTurn()
        {
            if (TurnEntity.Unpack(out var world, out var entity))
                throw new Exception("No battle");

            ecsContext.GetPool<MakeTurnTag>().Add(entity);
        }

        private void SetEcsCurrentTurnInfo(BattleTurnInfo info, TurnState state)
        {
            throw new System.NotImplementedException();
        }
        

        internal void DequeueHero(EcsPackedEntityWithWorld heroInstancePackedEntity)
        {
            var filter = ecsContext
                .Filter<BattleRoundInfo>()
                .End();

            var roundInfoPool = ecsContext.GetPool<BattleRoundInfo>();
            
            foreach(var roundEntity in filter)
            {
                ref var roundInfo = ref roundInfoPool.Get(roundEntity);

                var buffer = ListPool<RoundSlotInfo>.Get();

                buffer.AddRange(roundInfo.QueuedHeroes);
                var idx = buffer.FindIndex(x =>
                    x.HeroInstancePackedEntity.Equals(heroInstancePackedEntity));

                if (idx >= 0)
                    buffer.RemoveAt(idx);

                roundInfo.QueuedHeroes = buffer.ToArray();

                ListPool<RoundSlotInfo>.Add(buffer);
            }
        }


        internal void FinalizeTurn()
        {
            var buffer = ListPool<RoundSlotInfo>.Get();
            ref var round = ref GetEcsCurrentRound();
            buffer.AddRange(round.QueuedHeroes);

            if (buffer.Count > 0)
                buffer.RemoveAt(0);

            round.QueuedHeroes = buffer.ToArray();
            if (round.QueuedHeroes.Length == 0)
            {
                if (!RoundEntity.Unpack(out var world, out var entity))
                    throw new Exception("No Round");

                world.GetPool<DestroyTag>().Add(entity);
            }    

            ListPool<RoundSlotInfo>.Add(buffer);
        }


    }
}