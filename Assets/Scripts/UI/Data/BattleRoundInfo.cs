using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.UI.Data
{
    public struct BattleRoundInfo
    {
        private List<RoundSlotInfo> queuedHeroes;
        private RoundState state;

        private int round;
        public RoundSlotInfo CurrentAttacker
        {
            get
            {
                if (queuedHeroes != null && queuedHeroes.Count > 0)
                    return queuedHeroes.First();

                return default;
            }
        }

        public RoundState State => state;
        public int Round => round;
        public List<RoundSlotInfo> QueuedHeroes => queuedHeroes;

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
                $"{CurrentAttacker.HeroName}, " +
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
            var idx = queue.FindIndex(x => x.HeroId == target.Id);
            if (idx >= 0)
                queue.Remove(queue[idx]);
            queuedHeroes = queue;
        }

        internal void EnqueueHero(Hero hero)
        {
            var queue = queuedHeroes;
            queue.Add(RoundSlotInfo.Create(hero));
            queuedHeroes = queue;
        }

        internal void ResetQueue()
        {
            queuedHeroes.Clear();
        }

        internal void FinalizeTurn()
        {
            var queue = queuedHeroes;
            if (queue.Count > 0)
                queue.Remove(queue[0]);            
            queuedHeroes = queue;
        }
    }
}