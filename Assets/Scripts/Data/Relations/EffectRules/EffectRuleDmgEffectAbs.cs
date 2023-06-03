using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// эффект от урона изменить +- знач (пп)	Цель	Эффект	Значение	длительность (до след. N хода героя)
    /// </summary>
    public struct EffectRuleDmgEffectAbs : IBattleEffectRule
    {
        private readonly RelationEffectKey key;

        public RelationEffectKey Key => key;

        public EffectRuleDmgEffectAbs(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            DamageEffect = rawValues[2].ParseDamageEffect();
            Value = rawValues[3].ParseIntValue(0, true);
            TurnsCount = rawValues[4].ParseIntValue();

            key = new RelationEffectKey(SpecOption.NA, DamageEffect, DamageType.NA, RelationsEffectType.DmgEffectKey);
        }

        public RelationsEffectType EffectType => RelationsEffectType.DmgEffectAbs;
        public string[] Source { get; set; }
        public string Description { get; set; }
        
        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion
                
        /// <summary>
        /// Damage Effect will be boosted using the provided Value
        /// </summary>
        public DamageEffect DamageEffect { get; set; }  

        /// <summary>
        /// value will be used as an increment added to the current spec/bonus value
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// To use in UI just provide values for Id, HeroIcon
        /// </summary>
        /// <param name="id"></param>
        /// <param name="heroIcon"></param>
        /// <returns>Info to be used in the UI</returns>
        public RelationEffectInfo DraftEffectInfo(int id = -1, string heroIcon = "")
        {
            var text = Value > 0 ? $"+{Value}" : $"-{Value}";
            var icon = key.DamageEffect.IconCode();
            var iconColor = Value > 0 ? Color.green : Color.red;
            var info = new RelationEffectInfo()
            {
                Id = id,
                HeroIcon = heroIcon,
                EffectText = text,
                EffectIcon = icon,
                EffectIconColor = iconColor,
            };
            return info;
        }
    }
}