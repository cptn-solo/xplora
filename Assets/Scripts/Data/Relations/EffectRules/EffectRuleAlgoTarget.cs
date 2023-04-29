﻿namespace Assets.Scripts.Data
{
    public struct EffectRuleAlgoTarget : IBattleEffectRule
    {
        private readonly RelationEffectKey key;

        public RelationEffectKey Key => key;

        public EffectRuleAlgoTarget(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            TurnsCount = rawValues[2].ParseIntValue();

            key = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, DamageType.NA, EffectType);
        }

        public RelationsEffectType EffectType => RelationsEffectType.AlgoTarget;
        public string[] Source { get; set; }
        public string Description { get; set; }

        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion
    }
}