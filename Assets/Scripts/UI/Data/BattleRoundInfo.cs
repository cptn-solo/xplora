using System.Collections.Generic;

namespace Assets.Scripts.UI.Data
{
    public struct BattleRoundInfo
    {
        private List<Hero> queuedHeroes;
        private Hero currentAttacker;

        public Hero CurrentAttacker => currentAttacker;
        public List<Hero> QueuedHeroes => queuedHeroes;

        public static BattleRoundInfo Create()
        {
            BattleRoundInfo info = default;
            info.currentAttacker = Hero.Default;
            info.queuedHeroes = new();

            return info;
        }
    }
}