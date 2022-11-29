using Assets.Scripts.UI.Data;

namespace Assets.Scripts.UI.Data
{
    public struct AttackInfo
    {
        private Hero attacker;
        private Hero target;
        public Hero Attacker => attacker;
        public Hero Target => target;

        public bool Lethal => target.HealthCurrent <= 0;

        public static AttackInfo Create(Hero attacker, Hero target)
        {
            AttackInfo info = default;
            info.attacker = attacker;
            info.target = target;

            return info;
        }
    }
}