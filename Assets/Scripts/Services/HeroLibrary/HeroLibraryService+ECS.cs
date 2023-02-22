using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.Services
{
    public partial class HeroLibraryService // ECS
    {
        private EcsWorld ecsContext;

        private IEcsSystems ecsInitSystems;
        private IEcsSystems ecsSystems;

        public EcsPackedEntityWithWorld PlayerTeamEntity { get; internal set; }
        public EcsPackedEntityWithWorld EnemyTeamEntity { get; internal set; }

        public EcsPackedEntityWithWorld[] HeroConfigEntities { get; private set; } =
            new EcsPackedEntityWithWorld[0];

        private void StartEcsContext()
        {
            ecsContext = new EcsWorld();

            ecsInitSystems = new EcsSystems(ecsContext);
            ecsInitSystems
                .Add(new TeamInitSystem())
                .Inject(this)
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

        /// <summary>
        /// Creates entity for Hero config so this entity is 1st instance
        /// of a Hero of a kind
        /// </summary>
        /// <returns>reference to a struct attached as a component to an ecs entity</returns>
        private ref Hero ProcessEcsHeroConfig(int idx)
        {
            var heroPool = ecsContext.GetPool<Hero>();
            var filter = ecsContext.Filter<Hero>().End();

            foreach(var existingEntity in filter)
            {
                ref var existing = ref heroPool.Get(existingEntity);
                if (existing.Id == idx)
                    return ref existing;
            }

            var addedEntity = ecsContext.NewEntity();
            ref var added = ref heroPool.Add(addedEntity);
            added.Id = idx;
            added.HeroType = HeroType.Human;
            added.Inventory = Hero.DefaultInventory();
            added.Traits = Hero.DefaultTraits();
            added.Attack = Hero.DefaultAttack();
            added.Defence = Hero.DefaultDefence();

            var positionPool = ecsContext.GetPool<PositionComp>();
            ref var positionComp = ref positionPool.Add(addedEntity);
            positionComp.Position = GetEcsNextFreePosition();

            var buffer = ListPool<EcsPackedEntityWithWorld>.Get();

            buffer.AddRange(HeroConfigEntities);
            buffer.Add(ecsContext.PackEntityWithWorld(addedEntity));
            HeroConfigEntities = buffer.ToArray();

            ListPool<EcsPackedEntityWithWorld>.Add(buffer);



            return ref added;
        }

        private Tuple<int, BattleLine, int> GetEcsNextFreePosition()
        {
            var buffer = ListPool<int>.Get();

            var positionPool = ecsContext.GetPool<PositionComp>();
            var filter = ecsContext.Filter<PositionComp>().End();
            foreach (var entity in filter)
            {
                ref var positionComp = ref positionPool.Get(entity);
                if (positionComp.Position != null)
                    buffer.Add(positionComp.Position.Item3);
            }
            buffer.Sort();
            var idx = buffer.Count;
            for(int i = 0; i < buffer.Count; i++)
                if (buffer[i]> i)
                {
                    idx = i;
                    break;
                }

            ListPool<int>.Add(buffer);

            return new (-1, BattleLine.NA, idx);

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
        private Hero[] GetEcsNotInTeamHeroes(EcsPackedEntityWithWorld teamPackedEntity, bool aliveOnly)
        {
            if (!teamPackedEntity.Unpack(out var world, out var teamEntity))
                throw new Exception("No Team");

            var teamPool = world.GetPool<Team>();
            ref var team = ref teamPool.Get(teamEntity);

            var filter = world.Filter<Hero>().Inc<PositionComp>().End();
            var posPool = world.GetPool<PositionComp>();
            var heroPool = world.GetPool<Hero>();
            var buffer = ListPool<Hero>.Get();

            foreach (var entity in filter)
            {
                ref var posComp = ref posPool.Get(entity);
                ref var hero = ref heroPool.Get(entity);
                if (posComp.Position.Item1 != team.Id)
                    buffer.Add(hero);
            }

            var retval = buffer.ToArray();
            ListPool<Hero>.Add(buffer);

            return retval;

        }

        private Hero[] GetEcsTeamHeroes(EcsPackedEntityWithWorld teamPackeEntity)
        {
            if (!teamPackeEntity.Unpack(out var world, out var teamEntity))
                throw new Exception("No Team");

            var teamPool = world.GetPool<Team>();
            ref var team = ref teamPool.Get(teamEntity);

            var filter = world.Filter<Hero>().Inc<PositionComp>().End();
            var posPool = world.GetPool<PositionComp>();
            var heroPool = world.GetPool<Hero>();
            var buffer = ListPool<Hero>.Get();

            foreach(var entity in filter)
            {
                ref var posComp = ref posPool.Get(entity);
                ref var hero = ref heroPool.Get(entity);
                if (posComp.Position.Item1 == team.Id)
                    buffer.Add(hero);
            }

            var retval = buffer.ToArray();
            ListPool<Hero>.Add(buffer);

            return retval;
        }

        private Hero GetEcsHeroById(int heroId) =>
            GetEcsRefHero(heroId);

        private ref Hero GetEcsRefHero(Hero target) =>
            ref GetEcsRefHero(target.Id);

        private ref Hero GetEcsRefHero(int heroId)
        {
            var packedEntity = HeroConfigEntities[heroId];

            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception($"No Hero config for id {heroId}");

            var heroPool = ecsContext.GetPool<Hero>();
            ref var hero = ref heroPool.Get(entity);

            return ref hero;
        }

        private void RetireEcsHero(Hero target)
        {
            ref var hero = ref GetEcsRefHero(target);
            RetireEcsRefHero(ref hero);
        }

        public void RetireEcsRefHero(ref Hero target)
        {
            //TODO: revisit to handle clone instead of config itself
            var packedEntity = HeroConfigEntities[target.Id];

            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception($"No Hero config for id {target.Id}");

            var positionPool = world.GetPool<PositionComp>();

            ref var pos = ref positionPool.Get(entity);
            pos.Position = GetEcsNextFreePosition();
        }


        private void MoveEcsEnemyFrontLine(Hero target)
        {
            ref var hero = ref GetEcsRefHero(target);
            MoveEcsRefEnemyFrontLine(ref hero);
        }

        private void MoveEcsRefEnemyFrontLine(ref Hero target)
        {
            var packedEntity = HeroConfigEntities[target.Id];

            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception($"No Hero config for id {target.Id}");

            var positionPool = ecsContext.GetPool<PositionComp>();
            ref var pos = ref positionPool.Get(entity);
            pos.Position = new(1, BattleLine.Front, 0);
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
            return default;
        }

        private void MoveEcsHeroToPosition(EcsPackedEntityWithWorld packedEntity, Tuple<int, BattleLine, int> position)
        {

            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception($"No Hero config");

            var positionPool = world.GetPool<PositionComp>();

            ref var pos = ref positionPool.Get(entity);
            pos.Position = position;

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
    }
}