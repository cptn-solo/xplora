using System;
using System.Collections.Generic;
using Assets.Scripts.UI.Common;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct BattleTurnInfo
    {
        public TurnState State { get; set; }
        public Hero Attacker { get; set; }
        public Tuple<int, BattleLine, int> AttackerPosition { get; set; }
        public Hero Target { get; set; }
        public Tuple<int, BattleLine, int> TargetPosition { get; set; }
        public int Turn { get; set; }
        public int Damage { get; set; }
        public DamageEffect[] TargetEffects { get; set; }
        public DamageEffect[] AttackerEffects { get; set; }

        // effects state carries attackers hp and effects
        // while other states - targets hp and effects
        public int HealthCurrent { get; set; }
        public int Health { get; set; }
        public int Speed { get; set; }

        public List<BarInfo> BarsInfoBattle => new() {
            BarInfo.EmptyBarInfo(0, $"HP: {HealthCurrent}", Color.red, (float)HealthCurrent / Health),
            BarInfo.EmptyBarInfo(1, $"Speed: {Speed}", null, Speed / Mathf.Max(Speed, 10f)),
        };

        public Dictionary<DamageEffect, int> ActiveEffects { get; internal set; }

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
                $"{Attacker} " +
                $">>> {Target}";

            var attackerEff = "";
            foreach (var ef in AttackerEffects)
                attackerEff += $"+{ef}";

            var targetEff = "";
            foreach (var ef in TargetEffects)
                targetEff += $"+{ef}";

            if (targetEff.Length > 0 && ExtraDamage > 0)
                targetEff += $"{-ExtraDamage}";

            var completed = $"Ход #{Turn}: " +
                $"{State}, " +
                $"{Attacker}, " +
                $">>> {Target}, " +
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
                $"{Attacker}, " +
                $"-{Damage}" +
                $"[{attackerEff}]";
            
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
    }
}