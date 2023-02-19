using System;
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

        private IEcsSystems ecsInitSystems;
        private IEcsSystems ecsSystems;

        public EcsPackedEntityWithWorld BattleEntity { get; internal set; }

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
                .Add(new GarbageCollectorSystem())

#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
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
            if (!BattleEntity.Unpack(out var world, out var entity))
                throw new Exception("No Battle");

            var roundRefPool = world.GetPool<BattleRoundRefComp>();
            ref var roundRef = ref roundRefPool.Get(entity);

            if (!roundRef.RoundPackedEntity.Unpack(world, out var roundEntity))
                throw new Exception("No Round");

            var roundInfoPool = world.GetPool<BattleRoundInfo>();
            ref var roundInfo = ref roundInfoPool.Get(roundEntity);
            return ref roundInfo;
        }

        private ref BattleTurnInfo GetEcsCurrentTurn()
        {
            if (!BattleEntity.Unpack(out var world, out var entity))
                throw new Exception("No Battle");

            var turnRefPool = world.GetPool<BattleTurnRefComp>();
            ref var turnRef = ref turnRefPool.Get(entity);

            if (!turnRef.TurnPackedEntity.Unpack(world, out var turnEntity))
                throw new Exception("No Turn");

            var turnInfoPool = world.GetPool<BattleTurnInfo>();
            ref var turnInfo = ref turnInfoPool.Get(turnEntity);
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

        private void SetEcsCurrentTurnInfo(BattleTurnInfo info, TurnState state)
        {
            throw new System.NotImplementedException();
        }

        private void RemoveEcsCompletedRounds()
        {
            var filter = ecsContext.Filter<BattleRoundInfo>().End();
            var roundPool = ecsContext.GetPool<BattleRoundInfo>();
            foreach (var entity in filter)
            {
                ref var round = ref roundPool.Get(entity);
                if (round.State == RoundState.RoundCompleted)
                    roundPool.Del(entity);
            }
        }
        private void EnqueueEcsRound()
        {
            ref var battleInfo = ref GetEcsCurrentBattle();

            var roundEntity = ecsContext.NewEntity();

            var roundPool = ecsContext.GetPool<BattleRoundInfo>();

            ref var roundInfo = ref roundPool.Add(roundEntity);
            roundInfo.Round = ++battleInfo.LastRoundNumber;

            PrepareRound(ref roundInfo);

            battleInfo.LastRoundNumber = roundInfo.Round;
        }

        private int GetEcsLastRoundNumber()
        {
            ref var battleInfo = ref GetEcsCurrentBattle();
            return battleInfo.LastRoundNumber;
        }

        private int GetEcsRoundsCount()
        {
            var filter = ecsContext.Filter<BattleRoundInfo>().End();
            return filter.GetEntitiesCount();
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

            ListPool<RoundSlotInfo>.Add(buffer);
        }

        /// <summary>
        ///     Enqueue heroes 
        /// </summary>
        /// <param name="info">Draft round</param>
        /// <returns>Round with heroes</returns>
        internal void PrepareRound(ref BattleRoundInfo info)
        {
            info.State = RoundState.PrepareRound;

            ref var battleInfo = ref GetEcsCurrentBattle();

            var buffer = ListPool<RoundSlotInfo>.Get();
            var heroInstanceRefPool = ecsContext.GetPool<HeroInstanceRefComp>();
            var playerTeamTagPool = ecsContext.GetPool<PlayerTeamTag>();
            var namePool = ecsContext.GetPool<NameComp>();
            var speedPool = ecsContext.GetPool<SpeedComp>();

            foreach(var heroInstance in PlayerHeroes.Concat(EnemyHeroes))
            {
                if (!heroInstance.HeroInstancePackedEntity.Unpack(out var world, out var heroInstanceEntity))
                    continue;

                ref var heroInstanceRef = ref heroInstanceRefPool.Get(heroInstanceEntity);

                RoundSlotInfo slotInfo = new RoundSlotInfo()
                {
                    HeroInstancePackedEntity = heroInstance.HeroInstancePackedEntity,
                    HeroName = namePool.Get(heroInstanceEntity).Name,
                    Speed = speedPool.Get(heroInstanceEntity).Speed,
                    TeamId = playerTeamTagPool.Has(heroInstanceEntity) ?
                        battleInfo.PlayerTeam.Id :
                        battleInfo.EnemyTeam.Id
                };
                buffer.Add(slotInfo);
            }

            var orderedHeroes = buffer.OrderByDescending(x => x.Speed);
            ListPool<RoundSlotInfo>.Add(buffer);

            Dictionary<int, List<RoundSlotInfo>> speedSlots = new();
            foreach (var hero in orderedHeroes)
            {
                if (speedSlots.TryGetValue(hero.Speed, out var slots))
                    slots.Add(hero);
                else
                    speedSlots[hero.Speed] = new List<RoundSlotInfo>() { hero };
            }

            var speedKeys = speedSlots.Keys.OrderByDescending(x => x);

            var queue = ListPool<RoundSlotInfo>.Get();

            foreach (var speed in speedKeys)
            {
                var slots = speedSlots[speed];
                while (slots.Count() > 0)
                {
                    var choosenIdx = slots.Count() == 1 ? 0 : Random.Range(0, slots.Count());
                    queue.Add(slots[choosenIdx]);
                    slots.RemoveAt(choosenIdx);
                }
            }

            info.QueuedHeroes = queue.ToArray();

            ListPool<RoundSlotInfo>.Add(queue);

            info.State = RoundState.RoundPrepared;
        }

        private EcsFilter TeamTagFilter<T, L>(EcsWorld world) where T:struct where L: struct
        {
            return world.Filter<T>()
                        .Inc<L>()
                        .Exc<DeadTag>().End();
        }
        private EcsFilter TeamTagFilter<T>(EcsWorld world) where T : struct
        {
            return world.Filter<T>()
                        .Exc<DeadTag>().End();
        }

        internal void PrepareTurn()
        {
            var roundSlot = CurrentRound.QueuedHeroes[0];

            if (!roundSlot.HeroInstancePackedEntity.Unpack(out var world, out var heroInstanceEntity))
                throw new Exception("No Hero instance");

            var heroInstanceRefPool = world.GetPool<HeroInstanceRefComp>();
            ref var heroInstanceRef = ref heroInstanceRefPool.Get(heroInstanceEntity);

            var effectsPool = world.GetPool<EffectsComp>();
            ref var effectsComp = ref effectsPool.Get(heroInstanceEntity);

            var attacker = heroInstanceRef;
            var heroConfigRefPool = world.GetPool<HeroConfigRefComp>();
            ref var attackerConfigRef = ref heroConfigRefPool.Get(heroInstanceEntity);

            if (!attackerConfigRef.HeroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                throw new Exception("No Hero Config");

            var heroConfigPool = libWorld.GetPool<Hero>();
            ref var attackerCofig = ref heroConfigPool.Get(heroConfigEntity);

            if (effectsComp.SkipTurnActive)
            {
                var skippedInfo = BattleTurnInfo.Create(CurrentTurn.Turn, attackerCofig,
                    0, effectsComp.ActiveEffects.Keys.ToArray());
                SetTurnInfo(skippedInfo, TurnState.TurnSkipped);
            }
            else
            {
                var attackTeam = roundSlot.TeamId;
                ref var battleInfo = ref CurrentBattle;

                var targetEntity = -1;
                Hero targetConfig = default;

                var ranged = world.GetPool<RangedTag>().Has(heroInstanceEntity);
                if (ranged)
                {
                    var filter = battleInfo.PlayerTeam.Id == attackTeam ?
                        TeamTagFilter<EnemyTeamTag>(world) :
                        TeamTagFilter<PlayerTeamTag>(world);
                    var targets = filter.GetRawEntities();
                    targetEntity = targets.Length > 0 ?
                        targets[Random.Range(0, targets.Length)] :
                        -1;
                }
                else
                {
                    var filterFront = battleInfo.PlayerTeam.Id == attackTeam ?
                        TeamTagFilter<EnemyTeamTag, FrontlineTag>(world) :
                        TeamTagFilter<PlayerTeamTag, FrontlineTag>(world);

                    var filterBack = battleInfo.PlayerTeam.Id == attackTeam ?
                        TeamTagFilter<EnemyTeamTag, BacklineTag>(world) :
                        TeamTagFilter<PlayerTeamTag, BacklineTag>(world);

                    var frontTargets = filterFront.GetRawEntities();
                    var backTargets = filterBack.GetRawEntities();

                    // TODO: consider range (not yet imported/parsed)

                    targetEntity = frontTargets.Length > 0 ?
                        frontTargets[Random.Range(0, frontTargets.Length)] :
                        backTargets.Length > 0 ?
                        backTargets[Random.Range(0, backTargets.Length)] :
                        -1;
                }

                if (targetEntity != -1)
                {
                    ref var targetConfigRef = ref heroConfigRefPool.Get(targetEntity);

                    if (!targetConfigRef.HeroConfigPackedEntity.Unpack(out _, out var targetConfigEntity))
                        throw new Exception("No Hero Config");

                    ref var targetConfigTemp = ref heroConfigPool.Get(targetConfigEntity);

                    targetConfig = targetConfigTemp;
                }

                var turnInfo = BattleTurnInfo.Create(CurrentTurn.Turn, attackerCofig, targetConfig,
                    0, effectsComp.ActiveEffects.Keys.ToArray(), null);

                SetTurnInfo(turnInfo, targetEntity == -1 ?
                    TurnState.NoTargets : TurnState.TurnPrepared);
            }
        }
    }
}