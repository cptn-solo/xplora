using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

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

        public bool SkipTurnActive =>
            ActiveEffects.ContainsKey(DamageEffect.Frozing) ||
            ActiveEffects.ContainsKey(DamageEffect.Stunned);

        internal Hero EnqueEffect(DamageEffectInfo damageEffect)
        {
            var existing = ActiveEffects;
            
            var count = damageEffect.RoundOff - damageEffect.RoundOn;
            if (existing.TryGetValue(damageEffect.Effect, out var effect))
                existing[damageEffect.Effect] = count;
            else
                existing.Add(damageEffect.Effect, count);

            ActiveEffects = existing;

            return this;
        }

        internal Hero UseEffect(DamageEffect effect, out bool used)
        {
            var existing = ActiveEffects;

            used = false;

            if (existing.TryGetValue(effect, out var count))
            {
                if (count > 1)
                    existing[effect] = count - 1;
                else existing.Remove(effect);

                used = true;
            }

            ActiveEffects = existing;

            return this;
        }

        internal Hero ResetEffects()
        {
            var existing = ActiveEffects;
            existing.Clear();

            ActiveEffects = existing;

            return this;
        }
    }
}