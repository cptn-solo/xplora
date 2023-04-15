namespace Assets.Scripts.Data
{
    public struct HeroRelationEffectConfig
    {
        /// <summary>
        /// Config is effective for hero config with this id
        /// </summary>
        public int HeroId { get; set; }

        /// <summary>
        /// Effect can be applied only to a party with a given relations state with currently analized party
        /// </summary>
        public RelationState RelationState { get; set; }

        /// <summary>
        /// State (Attacking, Being Attacked, etc. of the currently analized party
        /// </summary>
        public RelationSubjectState SelfState { get; set; }
        
        /// <summary>
        /// currently not assigned by the configuration data, for future use
        /// </summary>
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