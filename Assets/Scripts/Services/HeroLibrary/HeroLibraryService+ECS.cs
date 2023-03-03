﻿using System;
using Assets.Scripts.Battle;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Google.Apis.Sheets.v4.Data;

namespace Assets.Scripts.Services
{
    using static UnityEngine.ParticleSystem;
    using static Zenject.SignalSubscription;
    using HeroPosition = Tuple<int, BattleLine, int>;

    public partial class HeroLibraryService // ECS
    {
        public EcsPackedEntityWithWorld PlayerTeamEntity { get; internal set; }
        public EcsPackedEntityWithWorld EnemyTeamEntity { get; internal set; }

        public EcsPackedEntityWithWorld[] HeroConfigEntities { get; private set; } =
            new EcsPackedEntityWithWorld[0];

        private Dictionary<HeroPosition, IHeroPosition> slots = new();

        private void StartEcsContext()
        {
            ecsWorld = new EcsWorld();

            ecsInitSystems = new EcsSystems(ecsWorld);
            ecsInitSystems
                .Add(new TeamInitSystem())
                .Inject(this)
                .Init();

            ecsRunSystems = new EcsSystems(ecsWorld);
            ecsRunSystems
                .Add(new LibraryBalanceUpdateSystem())
                .Add(new GarbageCollectorSystem())

#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
                .Inject(this)
                .Init();

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
        private ref Hero ProcessEcsHeroConfig(int idx)
        {
            var heroPool = ecsWorld.GetPool<Hero>();
            var filter = ecsWorld.Filter<Hero>().End();

            foreach (var existingEntity in filter)
            {
                ref var existing = ref heroPool.Get(existingEntity);
                if (existing.Id == idx)
                    return ref existing;
            }

            var addedEntity = ecsWorld.NewEntity();
            ref var added = ref heroPool.Add(addedEntity);
            added.Id = idx;
            added.HeroType = HeroType.Human;
            added.Inventory = Hero.DefaultInventory();
            added.Traits = Hero.DefaultTraits();
            added.Attack = Hero.DefaultAttack();
            added.Defence = Hero.DefaultDefence();

            var positionPool = ecsWorld.GetPool<PositionComp>();
            ref var positionComp = ref positionPool.Add(addedEntity);
            positionComp.Position = GetEcsNextFreePosition();

            var buffer = ListPool<EcsPackedEntityWithWorld>.Get();

            buffer.AddRange(HeroConfigEntities);
            buffer.Add(ecsWorld.PackEntityWithWorld(addedEntity));
            HeroConfigEntities = buffer.ToArray();

            ListPool<EcsPackedEntityWithWorld>.Add(buffer);

            return ref added;
        }

        private Tuple<int, BattleLine, int> GetEcsNextFreePosition()
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
        private EcsPackedEntityWithWorld[] GetEcsNotInTeamHeroes(EcsPackedEntityWithWorld teamPackedEntity, bool aliveOnly)
        {
            if (!teamPackedEntity.Unpack(out var world, out var teamEntity))
                throw new Exception("No Team");

            var teamPool = world.GetPool<Team>();
            ref var team = ref teamPool.Get(teamEntity);

            var filter = world.Filter<Hero>().Inc<PositionComp>().End();
            var posPool = world.GetPool<PositionComp>();
            var buffer = ListPool<EcsPackedEntityWithWorld>.Get();

            foreach (var entity in filter)
            {
                ref var posComp = ref posPool.Get(entity);
                if (posComp.Position.Item1 != team.Id)
                    buffer.Add(world.PackEntityWithWorld(entity));
            }

            var retval = buffer.ToArray();
            ListPool<EcsPackedEntityWithWorld>.Add(buffer);

            return retval;

        }

        private EcsPackedEntityWithWorld[] GetEcsTeamHeroes(EcsPackedEntityWithWorld teamPackeEntity)
        {
            if (!teamPackeEntity.Unpack(out var world, out var teamEntity))
                throw new Exception("No Team");

            var teamPool = world.GetPool<Team>();
            ref var team = ref teamPool.Get(teamEntity);

            var filter = world.Filter<Hero>().Inc<PositionComp>().End();
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

        internal void BindEcsHeroSlots(IHeroPosition[] buffer)
        {
            foreach (var slot in buffer)
                if (slots.TryGetValue(slot.Position, out _))
                    slots[slot.Position] = slot;
                else slots.Add(slot.Position, slot);
        }

        internal void CreateCards()
        {
            var positionPool = ecsWorld.GetPool<PositionComp>();
            var entityViewRefPool = ecsWorld.GetPool<EntityViewRef<Hero>>();
            var filter = ecsWorld.Filter<Hero>().Inc<PositionComp>().End();
            foreach (var entity in filter)
            {
                if (!entityViewRefPool.Has(entity))
                    entityViewRefPool.Add(entity);

                ref var entityViewRef = ref entityViewRefPool.Get(entity);
                if (entityViewRef.EntityView != null)
                {
                    entityViewRef.EntityView.UpdateData();
                    continue;
                }

                ref var pos = ref positionPool.Get(entity);
                var slot = slots[pos.Position];
                var card = HeroCardFactory(ecsWorld.PackEntityWithWorld(entity));
                card.DataLoader = GetDataForPackedEntity<Hero>;
                slot.Put(card.Transform);
                card.UpdateData();

                entityViewRef.EntityView = card;
            }
        }


        private EcsPackedEntityWithWorld? GetEcsHeroAtPosition(Tuple<int, BattleLine, int> position)
        {
            var positionPool = ecsWorld.GetPool<PositionComp>();
            var filter = ecsWorld.Filter<Hero>().Inc<PositionComp>().End();
            foreach (var entity in filter)
            {
                ref var pos = ref positionPool.Get(entity);
                if (pos.Position.Equals(position))
                    return ecsWorld.PackEntityWithWorld(entity);
            }
            return default;
        }

        private void MoveEcsHeroToPosition(EcsPackedEntityWithWorld packedEntity, Tuple<int, BattleLine, int> position)
        {

            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception($"No Hero config");

            var positionPool = world.GetPool<PositionComp>();

            ref var pos = ref positionPool.Get(entity);
            pos.Position = position;

            var slot = slots[pos.Position];

            var entityViewRefPool = world.GetPool<EntityViewRef<Hero>>();
            ref var entityViewRef = ref entityViewRefPool.Get(entity);
            slot.Put(entityViewRef.EntityView.Transform);
        }

        internal void BoostSpecOption(Hero eventHero, SpecOption specOption, int factor)
        {
            var packed = HeroConfigEntities[eventHero.Id];
            if (!packed.Unpack(out var world, out var entity))
                throw new Exception("No Hero Config");

            var heroConfigPool = world.GetPool<Hero>();
            ref var heroConfig = ref heroConfigPool.Get(entity);

            switch (specOption)
            {
                case SpecOption.DamageRange:
                    heroConfig.DamageMin += factor;
                    heroConfig.DamageMax += factor;
                    break;
                case SpecOption.DefenceRate:
                    heroConfig.DefenceRate += factor;
                    break;
                case SpecOption.AccuracyRate:
                    heroConfig.AccuracyRate += factor;
                    break;
                case SpecOption.DodgeRate:
                    heroConfig.DodgeRate += factor;
                    break;
                case SpecOption.Health:
                    heroConfig.Health += factor;
                    break;
                case SpecOption.Speed:
                    heroConfig.Speed += factor;
                    break;
                case SpecOption.UnlimitedStaminaTag: //NA for permanent boost
                    break;
                default:
                    break;
            }

        }

        internal void BoostTraitOption(Hero eventHero, HeroTrait traitOption, int factor)
        {
            var packed = HeroConfigEntities[eventHero.Id];
            if (!packed.Unpack(out var world, out var entity))
                throw new Exception("No Hero Config");

            var heroConfigPool = world.GetPool<Hero>();
            ref var heroConfig = ref heroConfigPool.Get(entity);

            switch (traitOption)
            {
                case HeroTrait.Hidden:
                case HeroTrait.Purist:
                case HeroTrait.Shrumer:
                case HeroTrait.Scout:
                case HeroTrait.Tidy:
                case HeroTrait.Soft:
                    {
                        var val = heroConfig.Traits[traitOption];
                        val.Level += factor;
                        heroConfig.Traits[traitOption] = val;
                    }
                    break;
                default:
                    break;
            }
        }

        public EcsPackedEntityWithWorld[] WrapForBattle(
            EcsPackedEntityWithWorld[] heroes, EcsWorld targetWorld = null)
        {
            var buffer = ListPool<EcsPackedEntityWithWorld>.Get();

            if (targetWorld == null)
                targetWorld = ecsWorld;

            foreach(var packed in heroes)
            {
                if (!packed.Unpack(out var world, out var configEntity))
                    throw new Exception("No Hero config");

                var entity = targetWorld.NewEntity();

                ref var configRef = ref targetWorld.GetPool<HeroConfigRefComp>().Add(entity);
                configRef.HeroConfigPackedEntity = packed;

                ref var heroConfig = ref world.GetPool<Hero>().Get(configEntity);

                ref var speedComp = ref targetWorld.GetPool<SpeedComp>().Add(entity);
                speedComp.Value = heroConfig.Speed;

                ref var healthComp = ref targetWorld.GetPool<HealthComp>().Add(entity);
                healthComp.Value = heroConfig.Health;

                ref var hpComp = ref targetWorld.GetPool<HPComp>().Add(entity);
                hpComp.Value = heroConfig.Health;

                buffer.Add(targetWorld.PackEntityWithWorld(entity));
            }

            var retval = buffer.ToArray();

            ListPool<EcsPackedEntityWithWorld>.Add(buffer);

            return retval;
        }
    }
}