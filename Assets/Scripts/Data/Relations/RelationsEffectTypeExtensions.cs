namespace Assets.Scripts.Data
{
    public static class RelationsEffectTypeExtensions
    {
        public static RelationsEffectClass EffectClass(this RelationsEffectType effectType)
            => effectType switch
            {
                RelationsEffectType.NA => RelationsEffectClass.NA,
                _ => RelationsEffectClass.Battle
            };
    }
}