namespace Assets.Scripts.Data
{
    public struct HeroRelationsEffectConfig
    {
        /// <summary>
        /// Config is effective for hero config with this id
        /// </summary>
        public int HeroId { get; set; }

        /// <summary>
        /// Effect can be applied only to a party with a given relations score with currently analized party
        /// </summary>
        public int TargetScore { get; set; }

        public RelationSubjectState SelfState { get; set; }
        public RelationSubjectState TargetState { get; set; }
        
        /// <summary>
        /// Basically, what is affected by an effect.
        /// </summary>
        public RelationsEffectType EffectType { get; set; }

        /// <summary>
        /// Rules to use while applying the effect
        /// </summary>
        public IEffectRule EffectRule { get; set; }

    }
}