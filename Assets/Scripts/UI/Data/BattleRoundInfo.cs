using System;
using System.Collections.Generic;

namespace Assets.Scripts.UI.Data
{
    public struct BattleRoundInfo
    {
        private List<Hero> queuedHeroes;
        private Hero currentAttacker;
        private RoundState state;
        public Hero CurrentAttacker => currentAttacker;
        public RoundState State => state;
        public List<Hero> QueuedHeroes => queuedHeroes;

        public static BattleRoundInfo Create()
        {
            BattleRoundInfo info = default;
            info.currentAttacker = Hero.Default;
            info.queuedHeroes = new();

            return info;
        }
        public BattleRoundInfo SetState(RoundState state)
        { 
            this.state = state;
            return this;
        }

        internal void DequeueHero(Hero target)
        {
            var queue = this.queuedHeroes;
            var idx = queue.FindIndex(x => x.Id == target.Id);
            if (idx >= 0)
                queue.RemoveAt(idx);
            this.queuedHeroes = queue;
        }
    }
}