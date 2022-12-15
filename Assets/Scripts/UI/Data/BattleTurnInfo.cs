namespace Assets.Scripts.UI.Data
{
    public struct BattleTurnInfo
    {
        private Hero attacker;
        private Hero target;
        private int turn;
        private int damage;
        private TurnState state;
        private bool effect;
        public TurnState State => state;
        public Hero Attacker => attacker;
        public Hero Target => target;
        public int Turn => turn;
        public int Damage => damage;

        // effects
        public bool Lethal { get; set; }
        public bool Critical { get; set; }
        public bool Dodged { get; set; }
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
                $"-{Damage}, " +
                $"[L:{Lethal} C:{Critical} D:{Dodged}]";
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
        public static BattleTurnInfo Create(int currentTurn, Hero attacker, int damage = 0)
        {
            BattleTurnInfo info = Create(currentTurn, attacker, Hero.Default, damage);
            info.effect = true;
            return info;
        }
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

        public BattleTurnInfo SetState(TurnState state)
        {
            this.state = state;
            return this;
        }
    }
}