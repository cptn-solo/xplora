using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Data
{
    public partial struct Hero // Effects
    {
        public bool RandomCriticalHit => CriticalHitRate.RatedRandomBool();
        public bool RandomDodge => DodgeRate.RatedRandomBool();
        public bool RandomAccuracy => AccuracyRate.RatedRandomBool();
        public int RandomDamage => Random.Range(DamageMin, DamageMax + 1);

        // casted effect resistance probability
        public bool RandomResistStun => ResistStunRate.RatedRandomBool();
        public bool RandomResistBleeding => ResistBleedRate.RatedRandomBool();
        public bool RandomResistPierced => false;
        public bool RandomResistBurning => ResistBurnRate.RatedRandomBool();
        public bool RandomResistFrozing => ResistFrostRate.RatedRandomBool();
        public bool EffectSkipTurn => Effects != null && Effects.Count > 0 &&
            Effects.FindIndex(x => x.TurnSkipped == true) >= 0;
        
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