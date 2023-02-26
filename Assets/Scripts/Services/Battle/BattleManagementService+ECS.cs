using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;
using UnityEngine;

namespace Assets.Scripts.Services
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public partial class BattleManagementService // ECS
    {
        private EcsWorld ecsContext;

        private readonly WaitForSeconds TickTimer = new(.2f);

        private IEcsSystems ecsInitSystems;
        private IEcsSystems ecsSystems;

        public EcsPackedEntityWithWorld BattleEntity { get; internal set; } //current battle
        public EcsPackedEntityWithWorld RoundEntity { get; internal set; } // current round
        public EcsPackedEntityWithWorld TurnEntity { get; internal set; } // current turn

        private Dictionary<HeroPosition, IHeroPosition> slots = new();

        public HeroPosition[] BattleFieldSlotsPositions => slots.Keys.ToArray();

        private void StartEcsContext()
        {
            PlayMode = BattleMode.NA;

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
                .Add(new BattleStartSystem()) // looks for playmode and a battle and starts it
                .Add(new BattleRetreatSystem()) // check if battle is retreated (canceled 
                .Add(new BattleWinCheckSystem()) // check if battle is already won 
                .Add(new BattleCompleteSystem()) // report won/retreated battle
                .Add(new BattleNotifyResultsSystem()) // if complete will notify UI and schedule battle shutdown
                // with battle in progress tag
                .Add(new BattleEnqueueRoundSystem()) // check queue length and add if needed
                .Add(new BattlePrepareRoundSystem()) // prepare added round (heroes queue)
                .Add(new BattleRoundStartSystem()) // picks 1st round and marks it as inprogress
                .Add(new BattleEnqueueTurnSystem()) // checks for empty turn and creates draft one
                // with DraftTag
                .Add(new BattleDraftTurnSystem())
                .Add(new BattleAssignAttackerSystem())
                .Add(new BattleAssignAttackerEffectsSystem())
                .Add(new BattleAssignTargetSystem())
                .Add(new BattleMarkTurnReadySystem()) // marks ready turns for autoplay
                .DelHere<DraftTag>()
                .Add(new BattleAutoMakeTurnSystem())
                // with MakeTurnTag, AttackTag
                .Add(new BattleApplyQueuedEffectsSystem()) // will skip next if died
                .Add(new BattleAttackSystem()) // tries to attack but can dodge/miss
                .Add(new BattleTryCastEffectsSystem()) // can pierce shield so goes 1st
                .Add(new BattleDealAttackDamageSystem())
                .DelHere<AttackTag>()
                .Add(new BattleCompleteTurnSystem()) // summs up turn info for UI
                // with CompletedTurnTag
                .Add(new BattleAutoProcessTurnSystem()) // for fast forward play
                .Add(new BattleFinalizeTurnSystem()) // removes turn and died heroes
                // dequeue fired items
                .Add(new BattleDequeueDiedHeroesSystem())
                .Add(new BattleDequeueCompletedRoundSystem())
                .Add(new GarbageCollectorSystem()) // will delete rounds and turns but not heroes
                .Add(new BattleTerminationSystem()) // will navigate from the battle screen stopping context

#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Inject(libraryManager)
                .Inject(prefs)
                .Init();

            StartCoroutine(BattleEcsRunloopCoroutine());
        }

        public void StopEcsContext()
        {
            PlayMode = BattleMode.NA;

            StopAllCoroutines();

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

        private void RetreatEcsBattle()
        {
            if (!BattleEntity.Unpack(out var world, out var entity))
                throw new Exception("No Battle");

            var pool = world.GetPool<RetreatTag>();

            if (!pool.Has(entity))
                pool.Add(entity);
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

        private void MakeEcsTurn()
        {
            if (!TurnEntity.Unpack(out var world, out var entity))
                throw new Exception("No turn");

            ecsContext.GetPool<MakeTurnTag>().Add(entity);
        }

        private void SetEcsTurnProcessed()
        {
            if (!TurnEntity.Unpack(out var world, out var entity))
                throw new Exception("No turn");

            ecsContext.GetPool<ProcessedTurnTag>().Add(entity);
        }

        #region Battle screen

        internal void BindEcsHeroSlots(IHeroPosition[] buffer)
        {
            foreach (var slot in buffer)
                if (slots.TryGetValue(slot.Position, out _))
                    slots[slot.Position] = slot;
                else slots.Add(slot.Position, slot);
        }

        internal delegate void OverlayToCardDelegate(IEntityView<Hero> card, IEntityView<BarsAndEffectsInfo> overlay);
        internal delegate void CardPlayerTeamDelegate(IEntityView<Hero> card, bool isPlayer);
        internal void CreateCards(OverlayToCardDelegate callback, CardPlayerTeamDelegate assignToPlayer)
        {
            var positionPool = ecsContext.GetPool<PositionComp>();
            var playerTeamPool = ecsContext.GetPool<PlayerTeamTag>();
            var entityViewRefPool = ecsContext.GetPool<EntityViewRef<Hero>>();
            var entityViewOverlayRefPool = ecsContext.GetPool<EntityViewRef<BarsAndEffectsInfo>>();
            var heroConfigRefPool = ecsContext.GetPool<HeroConfigRefComp>();
            var filter = ecsContext.Filter<HeroConfigRefComp>().Inc<PositionComp>().End();
            foreach (var entity in filter)
            {
                ref var pos = ref positionPool.Get(entity);
                var slot = slots[pos.Position];

                var packed = ecsContext.PackEntityWithWorld(entity);

                var card = HeroCardFactory(packed);
                card.DataLoader = GetHeroConfigForPackedEntity<Hero>;
                slot.Put(card.Transform);
                assignToPlayer(card, playerTeamPool.Has(entity));
                card.UpdateData();

                ref var entityViewRef = ref entityViewRefPool.Add(entity);
                entityViewRef.EntityView = card;

                var overlay = HeroOverlayFactory(packed);
                overlay.DataLoader = GetDataForPackedEntity<BarsAndEffectsInfo>;
                overlay.UpdateData();

                ref var entityViewOverlayRef = ref entityViewOverlayRefPool.Add(entity);
                entityViewOverlayRef.EntityView = overlay;

                callback?.Invoke(card, overlay);

            }
        }

        private void UnlinkCardRefs<T>()
        {
            var entityViewRefPool = ecsContext.GetPool<EntityViewRef<T>>();
            var filter = ecsContext.Filter<EntityViewRef<T>>().End();
            foreach (var entity in filter)
            {
                ref var entityViewRef = ref entityViewRefPool.Get(entity);
                entityViewRef.EntityView = null;
                entityViewRefPool.Del(entity);
            }
        }

        public T GetHeroConfigForPackedEntity<T>(EcsPackedEntityWithWorld? packed)
            where T : struct
        {
            if (packed == null || !packed.Value.Unpack(out var world, out var entity))
                return default;

            ref var heroConfigRef = ref world.GetPool<HeroConfigRefComp>().Get(entity);

            if (!heroConfigRef.HeroConfigPackedEntity.Unpack(out var libWorld, out var configEntity))
                return default;

            ref var heroConfig = ref libWorld.GetPool<T>().Get(configEntity);

            return heroConfig;
        }

        public T GetDataForPackedEntity<T>(EcsPackedEntityWithWorld? packed)
            where T : struct
        {
            if (packed != null && packed.Value.Unpack(out var world, out var entity))
                return world.GetPool<T>().Get(entity);

            return default;
        }

        internal void EnqueueEntityViewUpdate<T>(EcsPackedEntityWithWorld packedEntity)
            where T: struct
        {
            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception("No entity");

            ref var viewRef = ref world.GetPool<EntityViewRef<T>>().Get(entity);
            viewRef.EntityView.UpdateData();
        }
        internal void EnqueueEntityViewDestroy<T>(EcsPackedEntityWithWorld packedEntity)
            where T : struct
        {
            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception("No entity");

            ref var viewRef = ref world.GetPool<EntityViewRef<T>>().Get(entity);
            viewRef.EntityView.Destroy();
        }

        private EcsPackedEntityWithWorld? GetEcsHeroAtPosition(Tuple<int, BattleLine, int> position)
        {
            var positionPool = ecsContext.GetPool<PositionComp>();
            var filter = ecsContext.Filter<Hero>().Inc<PositionComp>().End();
            foreach (var entity in filter)
            {
                ref var pos = ref positionPool.Get(entity);
                if (pos.Position.Equals(position))
                    return ecsContext.PackEntityWithWorld(entity);
            }
            return null;
        }

        /// <summary>
        ///  Will trigger recreate of rounds with their respective hero queues
        /// </summary>
        private void DestroyEcsRounds()
        {
            var filter = ecsContext.Filter<BattleRoundInfo>().End();
            var destroyTagPool = ecsContext.GetPool<GarbageTag>();
            foreach (var entity in filter)
                destroyTagPool.Add(entity);
        }

        private void MoveEcsHeroToPosition(
            EcsPackedEntityWithWorld packedEntity,
            Tuple<int, BattleLine, int> position)
        {
            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception($"No Hero");

            var positionPool = world.GetPool<PositionComp>();

            ref var pos = ref positionPool.Get(entity);
            pos.Position = position;

            var slot = slots[pos.Position];

            var entityViewRefPool = world.GetPool<EntityViewRef<Hero>>();
            ref var entityViewRef = ref entityViewRefPool.Get(entity);
            slot.Put(entityViewRef.EntityView.Transform);

        }

        #endregion
    }
}