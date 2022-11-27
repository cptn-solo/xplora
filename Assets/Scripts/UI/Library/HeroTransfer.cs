using Assets.Scripts.UI.Data;
using System.Collections.Generic;

namespace Assets.Scripts
{
    using HeroDict = Dictionary<int, Hero>;

    public class HeroTransfer
    {
        public struct HeroTransaction
        {
            public Hero Hero;
            public HeroDict FromLine;
            public int FromIdx;
            public HeroDict ToLine;
            public int ToIdx;

        }
        private HeroTransaction heroTransaction = default;
        public Hero TransferHero => heroTransaction.Hero;

        public void Begin(HeroDict from, int fromIdx)
        {
            heroTransaction = new HeroTransaction
            {
                Hero = from[fromIdx],
                FromLine = from,
                FromIdx = fromIdx,
            };
        }

        public bool Commit(HeroDict toLine, int toIndex, BattleLine line)
        {
            if (heroTransaction.Hero.HeroType == HeroType.NA)
                return false;

            heroTransaction.ToLine = toLine;
            heroTransaction.ToIdx = toIndex;

            heroTransaction.FromLine.TakeHero(heroTransaction.FromIdx);

            var hero = heroTransaction.Hero;
            hero.Line = line;
            heroTransaction.ToLine.PutHero(hero, heroTransaction.ToIdx);

            heroTransaction = default;

            return true;
        }
        public bool Abort()
        {
            heroTransaction = default;

            return true;
        }


    }
}