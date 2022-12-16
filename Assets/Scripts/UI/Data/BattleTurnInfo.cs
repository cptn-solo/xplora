namespace Assets.Scripts.UI.Data
{
    public struct BattleTurnInfo
    {
        private Hero attacker;
        private Hero target;
        private int turn;
        private int damage;
        private TurnState state;
        private DamageEffect[] attackerEffects;
        private DamageEffect[] targetEffects;
        public TurnState State => state;
        public Hero Attacker => attacker;
        public Hero Target => target;
        public int Turn => turn;
        public int Damage => damage;
        public DamageEffect[] TargetEffects => targetEffects;
        public DamageEffect[] AttackerEffects => attackerEffects;

        // effects
        public bool Lethal { get; set; }
        public bool Critical { get; set; }
        public bool Dodged { get; set; }
        public bool Pierced { get; set; }
        public override string ToString()
        {
            var prepared = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{Attacker} " +
                $">>> {Target}";
            var completed = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{Attacker}, " +
                $">>> {Target}, " +
                $"-{Damage}" +
                $"[ " +
                (Lethal ? $"Lethal " : $"") + 
                (Critical ? $"Crit " : $"") +
                (Dodged ? $"Dodged " : $"") +
                (Pierced ? $"Pierced " : $"") +
                $"]";
            var effects = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{Attacker}, " +
                $"-{Damage}";
            var skipped = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{Attacker}";

            return State switch
            {
                TurnState.TurnPrepared => prepared,             
                TurnState.TurnEffects => effects,
                TurnState.TurnSkipped => skipped,
                TurnState.TurnCompleted => completed,
                _ => $"Ход #{Turn}: {State}"
            };
        }

        // constructors
        public static BattleTurnInfo Create(int currentTurn, Hero attacker,
            int damage = 0, DamageEffect[] attackerEffects = null)
        {
            BattleTurnInfo info = Create(currentTurn, attacker, Hero.Default, damage);
            info.attackerEffects = attackerEffects;
            return info;
        }
        public static BattleTurnInfo Create(int currentTurn, Hero attacker, Hero target, 
            int damage = 0, DamageEffect[] targetEffects = null)
        {
            BattleTurnInfo info = default;
            info.attacker = attacker;
            info.target = target;
            info.turn = currentTurn;
            info.damage = damage;
            info.state = TurnState.NA;
            info.attackerEffects = new DamageEffect[] { };
            info.targetEffects = targetEffects ?? (new DamageEffect[] { });

            return info;
        }

        public BattleTurnInfo SetState(TurnState state)
        {
            this.state = state;
            return this;
        }
    }
}