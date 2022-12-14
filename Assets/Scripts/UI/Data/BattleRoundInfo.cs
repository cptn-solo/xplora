using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.UI.Data
{
    public struct BattleRoundInfo
    {
        private List<Hero> queuedHeroes;
        private RoundState state;

        private int round;
        public Hero CurrentAttacker
        {
            get
            {
                if (queuedHeroes != null && queuedHeroes.Count > 0)
                    return queuedHeroes.First();

                return Hero.Default;
            }
        }

        public RoundState State => state;
        public int Round => round;
        public List<Hero> QueuedHeroes => queuedHeroes;

        public static BattleRoundInfo Create(int round)
        {
            BattleRoundInfo info = default;
            info.queuedHeroes = new();
            info.round = round;

            return info;
        }
        public BattleRoundInfo SetState(RoundState state)
        { 
            this.state = state;
            return this;
        }

        public override string ToString()
        {
            var full = $"Раунд #{Round}: " +
                $"{State}, " +
                $"очередь: {QueuedHeroes?.Count}, " +
                $"{CurrentAttacker.Name}, " +
                $"К{CurrentAttacker.TeamId}";
            
            return State switch
            {
                RoundState.RoundPrepared => full,
                RoundState.RoundInProgress => full,
                _ => $"Раунд #{Round}: {State}"
            };
        }

        internal void DequeueHero(Hero target)
        {
            var queue = queuedHeroes;
            var idx = queue.FindIndex(x => x.Id == target.Id);
            if (idx >= 0)
                queue.RemoveAt(idx);
            queuedHeroes = queue;
        }

        internal void UpdateHero(Hero target)
        {
            var queue = queuedHeroes;
            var idx = queue.FindIndex(x => x.Id == target.Id);
            if (idx >= 0)
                queue[idx] = target.ClearInactiveEffects(Round);
            queuedHeroes = queue;
        }

        internal void EnqueueHero(Hero hero)
        {
            var queue = this.queuedHeroes;
            queue.Add(hero);
            this.queuedHeroes = queue;
        }

        internal void ResetQueue()
        {
            QueuedHeroes.Clear();
        }
    }
}