using Assets.Scripts.UI.Data;
using System;

namespace Assets.Scripts.UI.Data
{
    public struct BattleTurnInfo
    {
        private Hero attacker;
        private Hero target;
        private int turn;
        private int damage;
        private TurnState state;
        public TurnState State => state;
        public Hero Attacker => attacker;
        public Hero Target => target;
        public int Turn => turn;
        public int Damage => damage;

        public bool Lethal => target.HealthCurrent <= 0;

        public static BattleTurnInfo Create(int currentTurn, Hero attacker, Hero target, int damage = 0)
        {
            BattleTurnInfo info = default;
            info.attacker = attacker;
            info.target = target;
            info.turn = currentTurn;
            info.damage = damage;
            info.state = TurnState.NA;

            return info;
        }

        internal void Increment()
        {
            turn++;
        }

        public BattleTurnInfo SetState(TurnState state)
        {
            this.state = state;
            return this;
        }
    }
}