using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using static Assets.Scripts.TeamManagementService;

namespace Assets.Scripts
{
    using HeroDict = Dictionary<int, Hero>;
    public partial class HeroLibraryManagementService
    {
        public struct HeroCardTransaction
        {
            public Hero Hero;
            public HeroDict FromLine; // for example library
            public int FromIdx;
            public HeroDict ToLine; //for example player team slots or enemy team slots
            public int ToIdx;

        }

        private HeroTransaction heroTransaction = default;
        public Hero TransferHero => heroTransaction.Hero;


        public void BeginHeroTransfer(HeroDict from, int fromIdx)
        {
            heroTransaction = new HeroTransaction
            {
                Hero = from[fromIdx],
                FromLine = from,
                FromIdx = fromIdx,
            };
        }

        public bool CommitHeroTransfer(HeroDict toLine, int toIndex)
        {
            if (heroTransaction.Hero.HeroType == HeroType.NA)
                return false;

            heroTransaction.ToLine = toLine;
            heroTransaction.ToIdx = toIndex;

            library.MoveHero(heroTransaction.Hero, heroTransaction.FromLine, heroTransaction.FromIdx, toLine, toIndex);

            heroTransaction = default;

            return true;
        }

        public bool AbortHeroTransfer()
        {
            heroTransaction = default;

            return true;
        }

    }
}

