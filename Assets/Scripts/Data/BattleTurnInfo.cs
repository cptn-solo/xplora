using Leopotam.EcsLite;

namespace Assets.Scripts.Data
{
    public struct BattleTurnInfo
    {
        public TurnState State { get; set; }
        public Hero AttackerConfig { get; set; }
        public EcsPackedEntityWithWorld? Attacker { get; set; }
        public Hero TargetConfig { get; set; }
        public EcsPackedEntityWithWorld? Target { get; set; }
        public int Turn { get; set; }

        // effects

        public override string ToString()
        {
            var prepared = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{AttackerConfig} " +
                $">>> {TargetConfig}";


            var completed = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{AttackerConfig}, " +
                $">>> {TargetConfig}, " +
                $"[ " +
                $"]";

            var effects = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{AttackerConfig}";
            
            var skipped = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{AttackerConfig}";

            return State switch
            {
                TurnState.TurnPrepared => prepared,             
                TurnState.TurnEffects => effects,
                TurnState.TurnSkipped => skipped,
                TurnState.TurnCompleted => completed,
                _ => $"Ход #{Turn}: {State}"
            };
        }
    }
}