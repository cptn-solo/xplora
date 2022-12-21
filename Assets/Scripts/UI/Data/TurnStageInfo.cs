using Assets.Scripts.UI.Data;
using System;

namespace Assets.Scripts.UI.Data
{
    public struct TurnStageInfo
    {
        public bool IsPierced { get; private set; }
        public int Damage { get; private set; }
        public string EffectText { get; private set; }

        public DamageEffect Effect { get; private set; }
        public static TurnStageInfo Dodged =>
            new() { EffectText = "Dodged!" };

        public static TurnStageInfo JustDamage(int damage) =>
            new() { Damage = damage };
        public static TurnStageInfo EffectDamage(DamageEffect effect, int damage) =>
            new() { Effect = effect, Damage = damage };
        internal static TurnStageInfo Pierced(int damage) =>
            new() { IsPierced = true, Damage = damage };

        internal static TurnStageInfo Critical(int damage) =>
            new() { EffectText = "Crit!", Damage = damage };
    }

}