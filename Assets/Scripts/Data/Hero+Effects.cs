using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public partial struct Hero // Effects
    {
        public bool RandomCriticalHit => CriticalHitRate.RatedRandomBool();
        public bool RandomDodge => DodgeRate.RatedRandomBool();
        public bool RandomAccuracy => AccuracyRate.RatedRandomBool();

        // casted effect resistance probability
        public bool RandomResistStun => ResistStunRate.RatedRandomBool();
        public bool RandomResistBleeding => ResistBleedRate.RatedRandomBool();
        public bool RandomResistPierced => false;
        public bool RandomResistBurning => ResistBurnRate.RatedRandomBool();
        public bool RandomResistFrozing => ResistFrostRate.RatedRandomBool();
    }
}