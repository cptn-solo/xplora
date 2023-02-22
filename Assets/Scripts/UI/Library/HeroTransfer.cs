using Assets.Scripts.Data;
using System;
using Leopotam.EcsLite;

namespace Assets.Scripts
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public class HeroTransfer
    {
        public struct HeroTransaction
        {
            public EcsPackedEntityWithWorld? Hero;
            public HeroPosition FromPosition;
            public HeroPosition ToPosition;
        }
        private HeroTransaction heroTransaction = default;
        public EcsPackedEntityWithWorld? TransferHero => heroTransaction.Hero;
        public int TeamId => heroTransaction.FromPosition.Item1;

        public void Begin(EcsPackedEntityWithWorld hero, HeroPosition from)
        {
            heroTransaction = new HeroTransaction
            {
                Hero = hero,
                FromPosition = from,
            };
        }

        public bool Commit(HeroPosition toPosition, out EcsPackedEntityWithWorld hero)
        {
            hero = default;
            if (heroTransaction.Hero == null)
                return false;

            heroTransaction.ToPosition = toPosition;

            hero = heroTransaction.Hero.Value;

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