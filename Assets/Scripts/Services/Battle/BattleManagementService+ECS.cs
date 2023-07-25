using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;


namespace Assets.Scripts.Services
{
    public partial class BattleManagementService // ECS
    {
        public BundleIconHost.Factory BundleIconFactory { get; internal set; }

        public EcsPackedEntityWithWorld BattleEntity { get; internal set; } //current battle
        public EcsPackedEntityWithWorld RoundEntity { get; internal set; } // current round
        public EcsPackedEntityWithWorld TurnEntity { get; internal set; } // current turn


        public void StartEcsContext()
        {
            TickTimer = new(.2f);

            PlayMode = BattleMode.NA;

            ecsWorld = new EcsWorld();

            ecsInitSystems = new EcsSystems(ecsWorld);
            ecsInitSystems
                .Add(new BattleInitSystem())
                .Inject(this)
                .Inject(libraryManager)
                .Init();

            ecsRunSystems = new EcsSystems(ecsWorld);
            ecsRunSystems
                .Add(new BattleHeroesInitSystem())
                .Add(new BattleHeroInstanceInit())
                .Add(new BattleUpdateHoverUnitSystem()) // show details on hover
                .Add(new BattlePotInitSystem())
                .Add(new BattleDeployHeroesSystem())
                .Add(new BattleDeployHeroOverlaysSystem())
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
                .Add(new BattlePrepareRevengeTurnsSystem())
                .Add(new BattleAssignAttackerSystem())
                .Add(new BattleAssignAttackerEffectsSystem())
                .Add(new BattleAssignAttackerRelationEffectsSystem()) // if attacker is hero (not enemy)
                                                                      // scans for relations and casts effects
                                                                      // on active attacker
                .Add(new BattleAssignTargetSystem())
                .Add(new BattleAssignTargetRelationEffectsSystem()) // if target is hero (not enemy)
                                                                    // scans for relations and casts effects
                                                                    // on active target 
                .Add(new BattleRelEffectProbeSystem()) //for all probes try cast a real effect
                .Add(new BattlePrepareRevengeEffectSystem())
                .Add(new BattlePrepareTargetEffectSystem()) //TODO: don't forget to reset this effect at some point
                .Add(new BattleEnemyTargetFocusHightlightSystem())
                .Add(new BattlePrepareRelEffectVisualSystem())
                .DelHere<RelEffectProbeComp>()
                .DelHere<DraftTag<EffectInstanceInfo>>() //effects (if any were spawned) will survive

                .Add(new BattleMarkTurnReadySystem()) // marks ready turns for autoplay
                .DelHere<DraftTag>()
                .Add(new BattleAutoMakeTurnSystem())
                
                // with MakeTurnTag, AttackTag
                .DelHere<UpdateTag<RelationEffectInfo>>()
                .Add(new BattleApplyQueuedEffectsSystem()) // will skip next if died
                                                           // with AttackTag
                .Add(new BattleAttackSystem()) // tries to attack but can dodge/miss
                .Add(new BattleTryCastEffectsSystem()) // can pierce shield so goes 1st
                .Add(new BattleDealAttackDamageSystem())
                .DelHere<AttackTag>()
                .Add(new BattleCompleteTurnSystem()) // summs up turn info for UI
                
                // with CompletedTurnTag+ScheduleVisualsTag (if not Fast Forward mode)
                .Add(new BattleScheduleRelationEffectVisualSystem<AttackerRef>())
                .Add(new BattleScheduleRelationEffectVisualSystem<TargetRef>())
                .DelHere<RelationEffectsPendingComp>()
                .Add(new BattleScheduleSceneVisualsQueuedEffectsSystem()) // for queued effects only
                .Add(new BattleScheduleSceneVisualsSystem()) // prepare visualization queue for attack
                .Add(new BattleScheduleSceneVisualsCompleteSystem()) // toggles AwaitVisualsTag to prevent reuse of the Schedule systems above
                
                // with AwaitVisualsTag
                .Add(new BattleRunSceneVisualsSystem()) // assigns visualizers so next systems could actually apply visuals
                .Add(new BattleSceneRelationEffectResetVisualSystem()) // subclass for effects container panel
                .Add(new BattleSceneRelationEffectCastVisualSystem()) // subclass for effects container panel
                .Add(new BattleSceneVisualSystem<DamageEffectVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<ArmorPiercedVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<TakingDamageVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<AttackDodgeVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<AttackMoveVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<AttackerAttackVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<AttackerMoveBackVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<HitVisualsInfo, Hero>())                
                .Add(new BattleSceneVisualSystem<DeathVisualsInfo, Hero>())                
                .Add(new BattleSceneVisualSystem<DeathVisualsInfo, BarsAndEffectsInfo>())
                .Add(new BattleSceneVisualSystem<EffectsBarVisualsInfo, BarsAndEffectsInfo>())
                .Add(new BattleSceneVisualSystem<HealthBarVisualsInfo, BarsAndEffectsInfo>())
                .Add(new BattleCompleteSceneVisualsSystem())
                .Add(new BattleAutoProcessTurnSystem()) // for fast forward play
                
                // with ProcessedTurnTag
                .Add(new BattleFinalizeTurnSystem()) // removes turn and died heroes
                
                // with FinalizedTurnTag
                .Add(new BattleClearVisualsForUsedFocusSystem()) // removes focus effect (revenge/target)
                .Add(new BattleClearVisualsForUsedRelationsEffectSystem()) // removes redundant battle effects visuals
                .Add(new BattleClearUsedRelationsEffectSystem()) // removes used effect instance entities (usage left == 0)
                .Add(new BattleReportUpdatedHeros()) // reports update back to the battle requester (raid)
                .Add(new BattleReportDeadHeroes()) // reports death back to the battle requester (raid)
                
                // dequeue fired items
                .Add(new BattleDequeueDiedHeroesSystem()) // retires died heroes
                .Add(new BattleDestroyDiedCardsSystem()) // for fastforward mode will destroy retired cards
                .DelHere<ProcessedHeroTag>()
                .Add(new BattleDetectCompletedRoundSystem()) // marks all empty rounds as garbage
                .Add(new BattleDequeueExpiredEffectFocusSystem()) // drops focus if it's target is dead or if the focus effect expired
                .Add(new BattleDequeueExpiredRelationEffectsSystem()) 
                .Add(new BattleDequeueCompletedRoundSystem())
                .Add(new GarbageCollectorSystem()) // will delete rounds and turns but not heroes
                .Add(new BattleTerminationSystem()) // will navigate from the battle screen stopping context

#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Inject(libraryManager)
                .Inject(raidService)
                .Inject(prefs)
                .Init();

            runloopCoroutine ??= StartCoroutine(RunloopCoroutine());
        }

        public void StopEcsContext()
        {
            PlayMode = BattleMode.NA;

            StopRunloopCoroutine();

            ecsRunSystems?.Destroy();
            ecsRunSystems = null;

            ecsInitSystems?.Destroy();
            ecsInitSystems = null;

            ecsWorld?.Destroy();
            ecsWorld = null;
        }


        public HeroInstanceRef[] PlayerHeroes => GetEcsPlayerHeroes();
        public HeroInstanceRef[] EnemyHeroes => GetEcsEnemyHeroes();

        public ref Hero GetHeroConfig(EcsPackedEntityWithWorld heroInstancePackedEntity)
        {
            if (!heroInstancePackedEntity.Unpack(out var world, out var heroInstanceEntity))
                throw new Exception("No Hero entity");

            ref var configRef = ref world.GetPool<HeroConfigRef>().Get(heroInstanceEntity);

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
            var filter = ecsWorld.Filter<BattleRoundInfo>().End();
            var pool = ecsWorld.GetPool<BattleRoundInfo>();

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

        private HeroInstanceRef[] GetEcsTaggedHeroes<T>() where T : struct
        {
            ref var battleInfo = ref GetEcsCurrentBattle();

            var filter = ecsWorld.Filter<HeroInstanceRef>()
                .Inc<T>()
                .Exc<DeadTag>().End();

            var buffer = ListPool<HeroInstanceRef>.Get();

            var heroRefPool = ecsWorld.GetPool<HeroInstanceRef>();

            foreach (var heroEntity in filter)
            {
                ref var heroRef = ref heroRefPool.Get(heroEntity);
                buffer.Add(heroRef);
            }
            var retval = buffer.ToArray();

            ListPool<HeroInstanceRef>.Add(buffer);

            return retval;
        }

        private HeroInstanceRef[] GetEcsPlayerHeroes() =>
            GetEcsTaggedHeroes<PlayerTeamTag>();

        private HeroInstanceRef[] GetEcsEnemyHeroes() =>
            GetEcsTaggedHeroes<EnemyTeamTag>();

        private void SetEcsTurnProcessed()
        {
            if (!TurnEntity.Unpack(out var world, out var entity))
                throw new Exception("No turn");

            world.GetPool<ProcessedTurnTag>().Add(entity);
        }

        #region Battle screen

        internal void BindEcsHeroSlots(Dictionary<HeroPosition, IHeroPosition> slots)
        {
            if (!BattleEntity.Unpack(out var world, out var entity))
                throw new Exception("No battle");

            ref var battleField = ref world.GetPool<BattleFieldComp>().Add(entity);
            battleField.Slots = slots;
        }


        public T GetHeroConfigForPackedEntity<T>(EcsPackedEntityWithWorld? packed)
            where T : struct
        {
            if (packed == null || !packed.Value.Unpack(out var world, out var entity))
                return default;

            ref var heroConfigRef = ref world.GetPool<HeroConfigRef>().Get(entity);

            if (!heroConfigRef.HeroConfigPackedEntity.Unpack(out var libWorld, out var configEntity))
                return default;

            ref var heroConfig = ref libWorld.GetPool<T>().Get(configEntity);

            return heroConfig;
        }

        private EcsPackedEntityWithWorld? GetEcsHeroAtPosition(HeroPosition position)
        {
            var positionPool = ecsWorld.GetPool<PositionComp>();
            var filter = ecsWorld.Filter<Hero>().Inc<PositionComp>().End();
            foreach (var entity in filter)
            {
                ref var pos = ref positionPool.Get(entity);
                if (pos.Position.Equals(position))
                    return ecsWorld.PackEntityWithWorld(entity);
            }
            return null;
        }

        /// <summary>
        ///  Will trigger recreate of rounds with their respective hero queues
        /// </summary>
        private void DestroyEcsRounds()
        {
            if (!BattleEntity.Unpack(out var battleWorld, out _))
                throw new Exception("No battle");

            var filter = battleWorld.Filter<BattleRoundInfo>().End();
            var destroyTagPool = battleWorld.GetPool<GarbageTag>();
            foreach (var entity in filter)
                destroyTagPool.Add(entity);

            ref var battle = ref CurrentBattle;
            battle.LastRoundNumber = -1;
        }

        private void MoveEcsHeroToPosition(
            EcsPackedEntityWithWorld packedEntity,
            HeroPosition position)
        {
            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception($"No Hero");

            if (!BattleEntity.Unpack(out var battleWorld, out var battleEntity))
                throw new Exception("No battle");

            ref var battleField = ref battleWorld.GetPool<BattleFieldComp>().Get(battleEntity);

            var positionPool = world.GetPool<PositionComp>();

            ref var pos = ref positionPool.Get(entity);
            pos.Position = position;

            var frontPool = world.GetPool<FrontlineTag>();
            var backPool = world.GetPool<BacklineTag>();

            if (backPool.Has(entity))
                backPool.Del(entity);

            if (frontPool.Has(entity))
                frontPool.Del(entity);

            if (position.Item2 == BattleLine.Front)
                frontPool.Add(entity);
            else
                backPool.Add(entity);

            var slot = battleField.Slots[pos.Position];

            var entityViewRefPool = world.GetPool<EntityViewRef<Hero>>();
            ref var entityViewRef = ref entityViewRefPool.Get(entity);
            slot.Put(entityViewRef.EntityView.Transform);

        }

        private bool TryGetBattleFieldSlots(out Dictionary<HeroPosition, IHeroPosition> slots)
        {
            slots = null;

            if (!BattleEntity.Unpack(out var world, out var entity))
                throw new Exception("No battle");

            var pool = world.GetPool<BattleFieldComp>();

            if (!pool.Has(entity))
                throw new Exception("No battle field");

            ref var battleField = ref pool.Get(entity);
            slots = battleField.Slots;

            return true;
        }

        internal void ShowAvailableTransferSlots(HeroPosition pos)
        {
            if (!TryGetBattleFieldSlots(out var slots))
                return;

            // toggle if position team matches slot team (moveonly across
            // the same team's slots
            foreach (var slot in slots)
                slot.Value.ToggleVisual(slot.Key.Item1 == pos.Item1);
        }

        internal void HideSlots()
        {
            if (!TryGetBattleFieldSlots(out var slots))
                return;

            foreach (var slot in slots)
                slot.Value.ToggleVisual(false);
        }


        #endregion
    }
}