using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Assets.Scripts.UI.Data
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    using HeroesDict = Dictionary<int, Hero>;
    using PositionedHeroes = Dictionary<Tuple<int, BattleLine, int>, int>;
    using HeroesPositions = Dictionary<int, Tuple<int, BattleLine, int>>;

    public struct HeroesLibrary
    {
        private HeroesDict heroById;
        private PositionedHeroes heroes;
        private HeroesPositions positions;

        private const int maxHeroes = 24;

        public Hero[] BattleHeroes
        {
            get
            {
                var combatantIds = heroes
                    .Where(x => x.Key.Item2 != BattleLine.NA)
                    .Select(x => x.Value).ToArray();
                var ret = heroById
                    .Where(x => combatantIds.Contains(x.Key))
                    .Select(x => x.Value);
                
                return ret.ToArray();
            }
        } 

        public static HeroesLibrary EmptyLibrary()
        {
            HeroesLibrary library = default;
            library.heroes = new();
            library.heroById = new();
            library.positions = new();

            return library;
        }

        public Hero HeroAtPosition(HeroPosition position)
        {
            if (heroes.TryGetValue(position, out var hero))
                return heroById[hero];

            return Hero.Default;

        }

        public Hero HeroById(int id)
        {
            if (heroById.TryGetValue(id, out var hero))
                return hero;

            return Hero.Default;
        }
        private HeroPosition NextFreePosion()
        {
            var pos = new HeroPosition(-1, BattleLine.NA, heroes.Count);
            return pos;
        }
        public bool GiveHero(Hero hero) {
            var pos = heroes.FirstFreeSlotIndex(x => x == -1);
            if (pos.Item3 == -1)
            {
                if (heroes.Count < maxHeroes)
                    pos = NextFreePosion();
                else
                    return false;
            }

            hero.TeamId = pos.Item1;
            hero.Line = pos.Item2;
            hero.Position = pos.Item3;

            SyncHeroPosition(hero, pos);

            return true;
        }
        public bool MoveHero(Hero hero, HeroPosition to)
        {
            hero.TeamId = to.Item1;
            hero.Line = to.Item2;
            hero.Position = to.Item3;

            SyncHeroPosition(hero, to);

            return true;
        }

        public bool UpdateHero(Hero hero)
        {
            if (!heroById.ContainsKey(hero.Id))
                return false;

            heroById[hero.Id] = hero;

            return true;
        }

        public void ResetHealthCurrent()
        {
            foreach (var item in heroById.Select(x => x.Key))
            {
                var hero = heroById[item];
                hero.HealthCurrent = hero.Health;
                heroById[item] = hero;
            }    
        }

        private readonly void SyncHeroPosition(Hero hero, HeroPosition pos)
        {
            if (positions.TryGetValue(hero.Id, out var oldPos))
            {
                positions[hero.Id] = pos;
                if (heroes.TryGetValue(oldPos, out _))
                    heroes.Remove(oldPos);
            }
            else
            {
                positions.Add(hero.Id, pos);
            }

            heroes.Add(pos, hero.Id);

            heroById[hero.Id] = hero;
        }
    }


}