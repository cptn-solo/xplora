using System;
using System.Collections.Generic;
using Assets.Scripts.UI.Common;
using UnityEngine;
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
        public int Damage { get; set; }
        public DamageEffect[] TargetEffects { get; set; }
        public DamageEffect[] AttackerEffects { get; set; }

        // effects
        public bool Lethal { get; set; }
        public bool Critical { get; set; }
        public bool Dodged { get; set; }
        public bool Pierced { get; set; }

        public int ExtraDamage { get; set; } // from effect applied by an attacker in current turn
        public override string ToString()
        {
            var prepared = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{AttackerConfig} " +
                $">>> {TargetConfig}";

            var attackerEff = "";
            if (AttackerEffects != null)
                foreach (var ef in AttackerEffects)
                    attackerEff += $"+{ef}";

            var targetEff = "";
            if (TargetEffects != null)
                foreach (var ef in TargetEffects)
                    targetEff += $"+{ef}";

            if (targetEff.Length > 0 && ExtraDamage > 0)
                targetEff += $"{-ExtraDamage}";

            var completed = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{AttackerConfig}, " +
                $">>> {TargetConfig}, " +
                $"-{Damage}" +
                $"[{targetEff}]" +
                $"[ " +
                (Lethal ? $"Lethal " : $"") + 
                (Critical ? $"Crit " : $"") +
                (Dodged ? $"Dodged " : $"") +
                (Pierced ? $"Pierced " : $"") +
                $"]";
            
            var effects = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{AttackerConfig}, " +
                $"-{Damage}" +
                $"[{attackerEff}]";
            
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