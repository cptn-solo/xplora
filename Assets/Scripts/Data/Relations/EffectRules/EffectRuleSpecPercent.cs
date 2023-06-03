using UnityEngine;

namespace Assets.Scripts.Data
{

    /// <summary>
    /// спека изменить до 100%/0%	Цель	Хар-ка	Значение	длительность (до след. N хода героя)
    /// </summary>
    public struct EffectRuleSpecPercent : IBattleEffectRule
    {
        private readonly RelationEffectKey key;

        public RelationEffectKey Key => key;

        public EffectRuleSpecPercent(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            SpecOption = rawValues[2].ParseSpecOption();
            Value = rawValues[3].ParseIntValue(0, true);
            TurnsCount = rawValues[4].ParseIntValue();

            key = new RelationEffectKey(SpecOption, DamageEffect.NA, DamageType.NA, RelationsEffectType.SpecKey);
        }

        public RelationsEffectType EffectType => RelationsEffectType.SpecPercent;
        public string[] Source { get; set; }
        public string Description { get; set; }
        
        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion
                
        /// <summary>
        /// Spec Option will be boosted using the provided Value
        /// </summary>
        public SpecOption SpecOption { get; set; }  

        /// <summary>
        /// value will be used as a new option value (untill the end of the effect)
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
            var text = Value switch { 0 => "Zero!", 100 => "100%", _ => $"{Value}%"};
            var icon = key.SpecOption.IconCode();
            var iconColor = Value <= 30 ? Color.red : Value >= 70 ? Color.green : Color.yellow;
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