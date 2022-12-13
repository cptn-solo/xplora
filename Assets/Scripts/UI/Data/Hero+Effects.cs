using UnityEngine;

namespace Assets.Scripts.UI.Data
{
    public partial struct Hero // Effects
    {
        public bool RandomCriticalHit => RatedRandomBool(CriticalHitRate);
        public bool RandomDodge => RatedRandomBool(DodgeRate);
        public bool RandomAccuracy => RatedRandomBool(AccuracyRate);
        public int RandomDamage => Random.Range(DamageMin, DamageMax + 1);

        // effect cast probability
        public bool RandomEffectStun => RatedRandomBool(20);
        public bool RandomEffectBleed => RatedRandomBool(10);
        public bool RandomEffectPierce => RatedRandomBool(15);
        public bool RandomEffectBurn => RatedRandomBool(10);
        public bool RandomEffectFrost => RatedRandomBool(10);

        // casted effect resistance probability
        public bool RandomResistStun => RatedRandomBool(ResistStunRate);
        public bool RandomResistBleeding => RatedRandomBool(ResistBleedRate);
        public bool RandomResistPierced => false;
        public bool RandomResistBurning => RatedRandomBool(ResistBurnRate);
        public bool RandomResistFrozing => RatedRandomBool(ResistFrostRate);
        
        public static bool RatedRandomBool(int rate)
        {
            var ret = false;
            if (rate == 100)
            {
                ret = true;
            }
            else if (rate >= 1 &&
                rate < 100)
            {
                ret = 0 == Random.Range(0, (int)Mathf.Ceil(100 / rate));
            }
            return ret;
        }

    }
}