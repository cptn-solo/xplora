using Assets.Scripts.UI.Data;
using System;

namespace Assets.Scripts.UI.Data
{
    public struct BattleTurnInfo
    {
        private Hero attacker;
        private Hero target;
        private int turn;
        public Hero Attacker => attacker;
        public Hero Target => target;
        public int Turn => turn;

        public bool Lethal => target.HealthCurrent <= 0;

        public static BattleTurnInfo Create(int currentTurn, Hero attacker, Hero target)
        {
            BattleTurnInfo info = default;
            info.attacker = attacker;
            info.target = target;
            info.turn = currentTurn;

            return info;
        }

        internal void Increment()
        {
            turn++;
        }
    }
}