namespace Assets.Scripts.UI.Data
{
    public struct DamageTypeInfo
    {
        public bool RandomEffectStun => 20.RatedRandomBool();
        public bool RandomEffectBleed => 10.RatedRandomBool();
        public bool RandomEffectPierce => 15.RatedRandomBool();
        public bool RandomEffectBurn => 10.RatedRandomBool();
        public bool RandomEffectFrost => 10.RatedRandomBool();
    }
}