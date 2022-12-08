using UnityEngine;

namespace Assets.Scripts.UI.Data
{
    public partial struct Hero // Effects
    {
        public bool RandomCriticalHit => RatedRandomBool(CriticalHitRate);
        public bool RandomDodge => RatedRandomBool(DodgeRate);
        public bool RandomAccuracy => RatedRandomBool(AccuracyRate);
        public int RandomDamage => Random.Range(DamageMin, DamageMax + 1);

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