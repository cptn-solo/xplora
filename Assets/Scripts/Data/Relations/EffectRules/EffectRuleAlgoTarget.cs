using UnityEngine;

namespace Assets.Scripts.Data
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

        /// <summary>
        /// To use in UI just provide values for Id, HeroIcon
        /// </summary>
        /// <param name="id"></param>
        /// <param name="heroIcon"></param>
        /// <returns>Info to be used in the UI</returns>
        public RelationEffectInfo DraftEffectInfo(int id = -1, string heroIcon = "")
        {
            var text = "";
            var icon = BundleIcon.MarkTarget;
            var iconColor = Color.Lerp(Color.yellow, Color.red, .5f);
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