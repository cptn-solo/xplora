using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.UI.Data
{
    public enum DamageEffect
    {
        NA = 0,
        Stunned = 100,
        Bleeding = 200,
        Pierced = 300,
        Burning = 400,
        Frozing = 500,
    }
    public struct DamageEffectInfo
    {
        public Hero Hero { get; private set; }
        public DamageEffect Effect { get; private set; }
        public int RoundOn { get; private set; }
        public int RoundOff { get; private set; }

        public DamageEffectInfo Cast(Hero hero, DamageEffect effect, int roundOn, int roundOff)
        {
            DamageEffectInfo info = default;
            
            info.Hero = hero;
            info.Effect = effect;
            info.RoundOn = roundOn;
            info.RoundOff = roundOff;

            return info;
        }
    }

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
                queue[idx] = target;
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