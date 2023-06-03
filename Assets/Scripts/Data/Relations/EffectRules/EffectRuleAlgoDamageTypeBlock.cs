using Assets.Scripts.ECS.Data;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.Scripts.Data
{
    public struct EffectRuleAlgoDamageTypeBlock : IBattleEffectRule
    {
        private readonly RelationEffectKey key;

        public RelationEffectKey Key => key;

        public EffectRuleAlgoDamageTypeBlock(string[] rawValues) : this()
        {
            Source = rawValues;
            TargetDomain = rawValues[1].ParseHeroDomain();
            DamageType = rawValues[2].ParseDamageType();
            Flag = rawValues[3].ParseIntValue(0, true);
            TurnsCount = rawValues[4].ParseIntValue();
            
            key = new RelationEffectKey(SpecOption.NA, DamageEffect.NA, DamageType, EffectType);
        }

        public RelationsEffectType EffectType => RelationsEffectType.AlgoDamageTypeBlock;
        public string[] Source { get; set; }
        public string Description { get; set; }
        
        #region IBattleEffectRule
        
        public HeroDomain TargetDomain { get; set; }
        public int TurnsCount { get; set; }
        
        #endregion

        /// <summary>
        /// Resistance/non-resistance to damage type being applied by the effect
        /// </summary>
        public DamageType DamageType { get; set; }
        
        /// <summary>
        /// 0 - set resistance to 0 (non-resistant)
        /// 1 - set resistance to 1 (resistant)
        /// </summary>
        public int Flag { get; set; }

        /// <summary>
        /// To use in UI just provide values for Id, HeroIcon
        /// </summary>
        /// <param name="id"></param>
        /// <param name="heroIcon"></param>
        /// <returns>Info to be used in the UI</returns>
        public RelationEffectInfo DraftEffectInfo(int id = -1, string heroIcon = "")
        {
            var text = Flag != 0 ? "Immune!" : "";
            var icon = key.DamageType.IconCode();
            var iconColor = Color.Lerp(Color.white, Color.blue, .5f);
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