using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// спека изменить на макс/мин	Цель	Урон	Флаг	длительность (до след. N хода героя)
    /// </summary>
    public struct EffectRuleSpecMaxMin : IBattleEffectRule
    {
        private readonly RelationEffectKey key;

        public RelationEffectKey Key => key;

        public EffectRuleSpecMaxMin(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            SpecOption = rawValues[2].ParseSpecOption();
            MaxMin = rawValues[3].ParseIntValue(0, true);
            TurnsCount = rawValues[4].ParseIntValue();
        
            key = new RelationEffectKey(SpecOption, DamageEffect.NA, DamageType.NA, RelationsEffectType.SpecKey);
        }

        public RelationsEffectType EffectType => RelationsEffectType.SpecMaxMin;
        public string[] Source { get; set; }
        public string Description { get; set; }

        public SpecOption SpecOption { get; set; }

        /// <summary>
        /// -1 => set minimum end of the ranged spec optin, 
        /// +1 => set maximium end of the ranged spec option
        /// </summary>
        public int MaxMin { get; set; }

        /// <summary>
        /// To use in UI just provide values for Id, HeroIcon
        /// </summary>
        /// <param name="id"></param>
        /// <param name="heroIcon"></param>
        /// <returns>Info to be used in the UI</returns>
        public RelationEffectInfo DraftEffectInfo(int id = -1, string heroIcon = "")
        {
            var text = "";
            var icon = key.SpecOption.IconCode(MaxMin);
            var iconColor = MaxMin == -1 ? Color.red : Color.green;
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

        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion

    }
}