using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.UI.Data
{
    public partial struct Hero // Effects
    {
        public bool RandomCriticalHit => true;// RatedRandomBool(CriticalHitRate);
        public bool RandomDodge => false;// RatedRandomBool(DodgeRate);
        public bool RandomAccuracy => true;// RatedRandomBool(AccuracyRate);
        public int RandomDamage => DamageMax;// Random.Range(DamageMin, DamageMax + 1);

        // effect cast probability
        public bool RandomEffectStun => true;// RatedRandomBool(20);
        public bool RandomEffectBleed => true;// RatedRandomBool(10);
        public bool RandomEffectPierce => true;// RatedRandomBool(15);
        public bool RandomEffectBurn => true;// RatedRandomBool(10);
        public bool RandomEffectFrost => true;// RatedRandomBool(10);

        // casted effect resistance probability
        public bool RandomResistStun => false;// RatedRandomBool(ResistStunRate);
        public bool RandomResistBleeding => false;// RatedRandomBool(ResistBleedRate);
        public bool RandomResistPierced => false;
        public bool RandomResistBurning => false;// RatedRandomBool(ResistBurnRate);
        public bool RandomResistFrozing => false;// RatedRandomBool(ResistFrostRate);

        public bool EffectSkipTurn => Effects != null && Effects.Count > 0 &&
            Effects.FindIndex(x => x.TurnSkipped == true) >= 0;
        
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

        public Hero ClearAllEffects()
        {
            Effects.Clear();
            return this;
        }
        public Hero ClearInactiveEffects(int maxRound)
        {
            if (Effects.Count > 0)
            {
                List<DamageEffectInfo> effects = new();
                effects.AddRange(Effects.Where(x => x.RoundOff >= maxRound));
                Effects = effects;
            }
            return this;

        }

    }
}