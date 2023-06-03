namespace Assets.Scripts.Data
{
    public interface IEffectRule
    {
        public RelationsEffectType EffectType { get; }
        public string[] Source { get; set; }
        public string Description { get; set; }
        
        RelationEffectKey Key { get; }

        /// <summary>
        /// To use in UI just provide values for Id, HeroIcon
        /// </summary>
        /// <param name="id"></param>
        /// <param name="heroIcon"></param>
        /// <returns>Info to be used in the UI</returns>
        public RelationEffectInfo DraftEffectInfo(int id = -1, string heroIcon = "");

    }
}