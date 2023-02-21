using Assets.Scripts.Data;
using System;

namespace Assets.Scripts
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public class HeroTransfer
    {
        public struct HeroTransaction
        {
            public Hero Hero;
            public HeroPosition FromPosition;
            public HeroPosition ToPosition;
        }
        private HeroTransaction heroTransaction = default;
        public Hero TransferHero => heroTransaction.Hero;
        public int TeamId => heroTransaction.FromPosition.Item1;

        public void Begin(Hero hero, HeroPosition from)
        {
            heroTransaction = new HeroTransaction
            {
                Hero = hero,
                FromPosition = from,
            };
        }

        public bool Commit(HeroPosition toPosition, out Hero hero)
        {
            hero = Hero.Default;
            if (heroTransaction.Hero.HeroType == HeroType.NA)
                return false;

            heroTransaction.ToPosition = toPosition;

            hero = heroTransaction.Hero;
            hero.TeamId = toPosition.Item1;
            hero.Line = toPosition.Item2;
            hero.Position = toPosition.Item3;

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