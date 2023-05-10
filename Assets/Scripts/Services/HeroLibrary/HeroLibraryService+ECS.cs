using System;
using Assets.Scripts.Battle;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;
using Assets.Scripts.ECS;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public partial class HeroLibraryService // ECS
    {
        public EcsPackedEntityWithWorld LibraryEntity { get; internal set; }
        public EcsPackedEntityWithWorld PlayerTeamEntity { get; internal set; }
        public EcsPackedEntityWithWorld EnemyTeamEntity { get; internal set; }

        public EcsPackedEntityWithWorld[] HeroConfigEntities { get; private set; } =
            new EcsPackedEntityWithWorld[0];


        private void StartEcsContext()
        {
            ecsWorld = new EcsWorld();

            ecsInitSystems = new EcsSystems(ecsWorld);
            ecsInitSystems
                .Add(new LibraryInitSystem())
                .Add(new TeamInitSystem())
                .Inject(this)
                .Init();

            ecsRunSystems = new EcsSystems(ecsWorld);
            ecsRunSystems

                .DelHere<DeadTag>() // battle kills doesn't matter for the library, just ignore the tag

                .Add(new LibraryDeployCardsSystem())

                // with UpdateTag<MovedTag>
                .Add(new LibraryHandleHeroMoveSystem())
                .DelHere<UpdateTag<MovedTag>>()

                // with UpdateTag<RelationsMatrixComp>
                .Add(new LibraryUpdatePlayerTeamRelationContextSystem())
                .DelHere<UpdateTag<RelationsMatrixComp>>()

                // with UpdateTag<SelectedTag>
                .Add(new LibraryUpdateCardSelectionSystem())
                .DelHere<UpdateTag<SelectedTag>>()

                .Add(new LibraryUpdateCardHoverSystem())
                .Add(new LibraryUpdateCardsSystem())

                .Add(new LibraryBalanceUpdateSystem())
                .Add(new GarbageCollectorSystem())

#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Init();

            TickTimer = null;
            runloopCoroutine ??= StartCoroutine(RunloopCoroutine());
        }

        private void StopEcsContext()
        {
            StopRunloopCoroutine();

            ecsRunSystems?.Destroy();
            ecsRunSystems = null;

            ecsInitSystems?.Destroy();
            ecsInitSystems = null;

            ecsWorld?.Destroy();
            ecsWorld = null;
        }

        /// <summary>
        /// Creates entity for Hero config so this entity is 1st instance
        /// of a Hero of a kind
        /// </summary>
        /// <returns>reference to a struct attached as a component to an ecs entity</returns>
        private ref Hero HeroConfigProcessor(int idx)
        {
            var configPool = ecsWorld.GetPool<Hero>();
            var configFilter = ecsWorld.Filter<Hero>().End();

            var configRefPool = ecsWorld.GetPool<HeroConfigRefComp>();
            var configRefFilter = ecsWorld.Filter<HeroConfigRefComp>().End();

            EcsPool<UpdateTag> updateTagPool = ecsWorld.GetPool<UpdateTag>();

            foreach (var existingInstance in configRefFilter)
            {
                ref var existingConfigRef = ref configRefPool.Get(existingInstance);
                
                if (!existingConfigRef.Packed.Unpack(out _, out var existingConfigEntity))
                    throw new Exception("No config for instance");

                ref var existingConfig = ref configPool.Get(existingConfigEntity);

                if (existingConfig.Id == idx)
                {
                    if (!updateTagPool.Has(existingInstance))
                        updateTagPool.Add(existingInstance);

                    return ref existingConfig;
                }
            }

            var configEntity = ecsWorld.NewEntity();
            var instanceEntity = ecsWorld.NewEntity();
            var configEntityPacked = ecsWorld.PackEntityWithWorld(configEntity);
            ref var configRef = ref configRefPool.Add(instanceEntity);
            configRef.HeroConfigPackedEntity = configEntityPacked;

            ref var addedConfig = ref configPool.Add(configEntity);
            addedConfig.Id = idx;
            addedConfig.HeroType = HeroType.Human;
            addedConfig.Inventory = Hero.DefaultInventory();
            addedConfig.Traits = Hero.DefaultTraits();
            addedConfig.Kinds = Hero.DefaultKinds();
            addedConfig.Attack = Hero.DefaultAttack();
            addedConfig.Defence = Hero.DefaultDefence();

            var positionPool = ecsWorld.GetPool<PositionComp>();
            ref var positionComp = ref positionPool.Add(instanceEntity);
            positionComp.Position = GetEcsNextFreePosition();

            var buffer = ListPool<EcsPackedEntityWithWorld>.Get();

            buffer.AddRange(HeroConfigEntities);
            buffer.Add(ecsWorld.PackEntityWithWorld(configEntity));
            HeroConfigEntities = buffer.ToArray();

            ListPool<EcsPackedEntityWithWorld>.Add(buffer);

            if (!updateTagPool.Has(instanceEntity))
                updateTagPool.Add(instanceEntity);

            return ref addedConfig;
        }

        private HeroPosition GetEcsNextFreePosition()
        {
            var buffer = ListPool<int>.Get();

            var positionPool = ecsWorld.GetPool<PositionComp>();
            var filter = ecsWorld.Filter<PositionComp>().End();
            foreach (var entity in filter)
            {
                ref var positionComp = ref positionPool.Get(entity);
                if (positionComp.Position != null)
                    buffer.Add(positionComp.Position.Item3);
            }
            buffer.Sort();
            var idx = buffer.Count;
            for (int i = 0; i < buffer.Count; i++)
                if (buffer[i] > i)
                {
                    idx = i;
                    break;
                }

            ListPool<int>.Add(buffer);

            return new(-1, BattleLine.NA, idx);

        }

        private ref Team GetEcsPlayerTeam()
        {
            if (!PlayerTeamEntity.Unpack(out var world, out var entity))
                throw new Exception("No Player Team");

            var teamPool = world.GetPool<Team>();
            ref var team = ref teamPool.Get(entity);

            return ref team;
        }

        private ref Team GetEcsEnemyTeam()
        {
            if (!EnemyTeamEntity.Unpack(out var world, out var entity))
                throw new Exception("No Enemy Team");

            var teamPool = world.GetPool<Team>();
            ref var team = ref teamPool.Get(entity);

            return ref team;

        }

        private EcsPackedEntityWithWorld[] GetEcsEnemyDomainHeroInstances()
        {
            if (!LibraryEntity.Unpack(out var world, out _))
                throw new Exception("No Library");

            var filter = world.Filter<HeroConfigRefComp>().Inc<PositionComp>().End();
            var buffer = ListPool<EcsPackedEntityWithWorld>.Get();
            var configPool = world.GetPool<Hero>();
            var configRefPool = world.GetPool<HeroConfigRefComp>();

            foreach (var entity in filter)
            {
                ref var configRef = ref configRefPool.Get(entity);
                if (configRef.Packed.Unpack(out _, out var configEntity))
                { 
                    ref var hero = ref configPool.Get(configEntity);
                    if (hero.Domain == HeroDomain.Enemy)
                        buffer.Add(world.PackEntityWithWorld(entity));                    
                }
            }

            var retval = buffer.ToArray();
            ListPool<EcsPackedEntityWithWorld>.Add(buffer);

            return retval;

        }

        private EcsPackedEntityWithWorld[] GetEcsTeamHeroInstances(EcsPackedEntityWithWorld teamPackeEntity)
        {
            if (!teamPackeEntity.Unpack(out var world, out var teamEntity))
                throw new Exception("No Team");

            var teamPool = world.GetPool<Team>();
            ref var team = ref teamPool.Get(teamEntity);

            var filter = world.Filter<HeroConfigRefComp>().Inc<PositionComp>().End();
            var posPool = world.GetPool<PositionComp>();
            var buffer = ListPool<EcsPackedEntityWithWorld>.Get();

            foreach (var entity in filter)
            {
                ref var posComp = ref posPool.Get(entity);
                if (posComp.Position.Item1 == team.Id)
                    buffer.Add(world.PackEntityWithWorld(entity));
            }

            var retval = buffer.ToArray();
            ListPool<EcsPackedEntityWithWorld>.Add(buffer);

            return retval;
        }

        internal void BindEcsHeroSlots(Dictionary<HeroPosition, IHeroPosition> slots)
        {
            if (!LibraryEntity.Unpack(out var world, out var entity))
                throw new Exception("No battle");

            ref var battleField = ref world.GetPool<LibraryFieldComp>().Add(entity);
            battleField.Slots = slots;
        }

        private void MoveEcsHeroToPosition(EcsPackedEntityWithWorld packedEntity, HeroPosition position)
        {

            if (!LibraryEntity.Unpack(out var world, out var libEntity))
                throw new Exception("No library");

            ref var libraryFiled = ref world.GetPool<LibraryFieldComp>().Get(libEntity);

            if (!packedEntity.Unpack(out _, out var entity))
                throw new Exception($"No Hero config");

            var positionPool = world.GetPool<PositionComp>();

            ref var pos = ref positionPool.Get(entity);
            pos.PrevPosition = pos.Position;
            pos.Position = position;

            var slot = libraryFiled.Slots[pos.Position];

            var entityViewRefPool = world.GetPool<EntityViewRef<Hero>>();
            ref var entityViewRef = ref entityViewRefPool.Get(entity);
            slot.Put(entityViewRef.EntityView.Transform);

            var moveTagPool = world.GetPool<UpdateTag<MovedTag>>();
            if (!moveTagPool.Has(entity))
                moveTagPool.Add(entity);

            ClearEcsHeroSelection();
        }

        internal void SetEcsSelectedHero(EcsPackedEntityWithWorld? packedEntity)
        {
            if (packedEntity == null || !packedEntity.Value.Unpack(out var world, out var entity))
                return;

            var selectedPool = world.GetPool<SelectedTag>();
            var selectedFilter = world.Filter<SelectedTag>().End();
            
            foreach (var selectedEntity in selectedFilter)
                selectedPool.Del(selectedEntity);                    

            selectedPool.Add(entity);
            var pool = world.GetPool<UpdateTag<SelectedTag>>();

            if (!pool.Has(entity))
                pool.Add(entity);            
        }

        private void SetEcsRelationScore(EcsPackedEntityWithWorld hero, float value)
        {
            Debug.Log($"SetEcsRelationScore {value}");

            if (!hero.Unpack(out var libWorld, out var entity))
                throw new Exception("No relation party");

            var selectedFilter = libWorld.Filter<SelectedTag>().End();
            var matrixFilter = libWorld.Filter<RelationsMatrixComp>().End();
            var matrixPool = ecsWorld.GetPool<RelationsMatrixComp>();

            foreach (var selectedEntity in selectedFilter)
            {
                foreach (var matrixEntity in matrixFilter)
                {
                    ref var matrixComp = ref matrixPool.Get(matrixEntity);
                    if (!matrixComp.Matrix.TryGetValue(new RelationsMatrixKey(hero, libWorld.PackEntityWithWorld(selectedEntity)), out var scoreEntityPacked))
                        throw new Exception("No score ref in the relations matrix");

                    if (!scoreEntityPacked.Unpack(out _, out var scoreEntity))
                        throw new Exception("Stale score entity");

                    ecsWorld.SetIntValue<RelationScoreTag>((int)value, scoreEntity);
                }
            }
        }

        private void ClearEcsHeroSelection()
        {
            if (!LibraryEntity.Unpack(out var world, out _))
                throw new Exception("No library");

            var selectedFilter = world.Filter<SelectedTag>().Inc<EntityViewRef<Hero>>().End();
            var selectedPool = world.GetPool<SelectedTag>();
            var updateSelectionPool = world.GetPool<UpdateTag<SelectedTag>>();

            foreach (var entity in selectedFilter)
            {
                selectedPool.Del(entity);
                if (!updateSelectionPool.Has(entity))
                    updateSelectionPool.Add(entity);                
            }
        }

        public EcsPackedEntityWithWorld[] WrapForBattle(
            EcsPackedEntityWithWorld[] configRefsPacked, EcsWorld targetWorld = null)
        {
            var buffer = ListPool<EcsPackedEntityWithWorld>.Get();

            var needNewEntities = targetWorld != null;
            targetWorld ??= ecsWorld;


            foreach(var packed in configRefsPacked)
            {
                if (!packed.Unpack(out _, out var configRefEntity))
                    throw new Exception("No Hero config ref");

                var libConfigRefPool = ecsWorld.GetPool<HeroConfigRefComp>();
                ref var heroConfigRef = ref libConfigRefPool.Get(configRefEntity);

                if (!heroConfigRef.Packed.Unpack(out _, out var configEntity))
                    throw new Exception("No hero config");

                var entity = needNewEntities ? targetWorld.NewEntity() : configRefEntity;

                if (needNewEntities)
                {
                    ref var configRef = ref targetWorld.GetPool<HeroConfigRefComp>().Add(entity);
                    configRef.HeroConfigPackedEntity = heroConfigRef.Packed;
                }

                ref var heroConfig = ref ecsWorld.GetPool<Hero>().Get(configEntity);
                
                targetWorld.SetIntValue<SpeedTag>(heroConfig.Speed, entity);
                targetWorld.SetIntValue<HealthTag>(heroConfig.Health, entity);
                targetWorld.SetIntValue<HpTag>(heroConfig.Health, entity);
                targetWorld.SetIntValue<DefenceRateTag>(heroConfig.DefenceRate, entity);
                targetWorld.SetIntValue<CritRateTag>(heroConfig.CriticalHitRate, entity);
                targetWorld.SetIntValue<AccuracyRateTag>(heroConfig.AccuracyRate, entity);
                targetWorld.SetIntValue<DodgeRateTag>(heroConfig.DodgeRate, entity);
                targetWorld.SetValue<IntRangeValueComp<DamageRangeTag>, IntRange>(new IntRange(heroConfig.DamageMin, heroConfig.DamageMax), entity);
                targetWorld.SetValue<NameValueComp<IconTag>, string>(heroConfig.IconName, entity);
                targetWorld.SetValue<NameValueComp<NameTag>, string>(heroConfig.Name, entity);
                targetWorld.SetValue<NameValueComp<IdleSpriteTag>, string>(heroConfig.IdleSpriteName, entity);

                buffer.Add(targetWorld.PackEntityWithWorld(entity));
            }

            var retval = buffer.ToArray();

            ListPool<EcsPackedEntityWithWorld>.Add(buffer);

            return retval;
        }        

        internal void DestroyEcsLibraryField()
        {
            if (!LibraryEntity.Unpack(out var world, out var entity))
                return;

            var pool = world.GetPool<LibraryFieldComp>();

            if (pool.Has(entity))
                pool.Del(entity);
        }

        internal Hero GetHeroConfigForLibraryHeroInstance(EcsPackedEntityWithWorld? packed)
        {
            if (packed != null && packed.Value.Unpack(out var world, out var entity))
            {
                var configRefPool = world.GetPool<HeroConfigRefComp>();
                if (configRefPool.Has(entity))
                {
                    ref var configRef = ref configRefPool.Get(entity);
                    if (configRef.Packed.Unpack(out _, out var configEntity))
                    {
                        var configPool = world.GetPool<Hero>();
                        if (configPool.Has(configEntity))
                            return configPool.Get(configEntity);
                    }
                }

            }

            return default;
        }

        public EcsPackedEntityWithWorld GetHeroConfigForLibraryHeroInstance(
            EcsPackedEntityWithWorld libHeroInstance,
            out Hero heroConfig)
        {
            heroConfig = default;

            if (!libHeroInstance.Unpack(out var libWorld, out var instanceEntity))
                throw new Exception("No hero config ref");
            
            var configRefPool = libWorld.GetPool<HeroConfigRefComp>();
            ref var configRef = ref configRefPool.Get(instanceEntity);

            if (!configRef.Packed.Unpack(out _, out var libHeroConfig))
                throw new Exception("No hero config");

            var heroPool = libWorld.GetPool<Hero>();
            heroConfig = heroPool.Get(libHeroConfig);

            return configRef.Packed;
        }

    }
}