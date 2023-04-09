namespace Assets.Scripts.Data
{
    public interface IEffectRule
    {
        public RelationsEffectType EffectType { get; }
        public string[] Source { get; set; }
        public string Description { get; set; }
    }
}