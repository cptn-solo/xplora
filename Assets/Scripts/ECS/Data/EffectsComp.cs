using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Assets.Scripts.ECS.Data
{
    public struct ActiveEffectComp
    {
        public DamageEffect Effect;
        public int EffectDamage;
        public EcsPackedEntityWithWorld Subject;
        public int TurnAttached;
        public readonly bool SkipTurn
        {
            get => Effect switch
            {
                DamageEffect.Frozing => true,
                DamageEffect.Stunned => true,
                _ => false
            };
        }
    }

    public struct EffectsComp
    {
        public Dictionary<DamageEffect, int> ActiveEffects { get; internal set; }
        public bool SkipTurnActive =>
            ActiveEffects.ContainsKey(DamageEffect.Frozing) ||
            ActiveEffects.ContainsKey(DamageEffect.Stunned);

        internal EffectsComp EnqueEffect(DamageEffectInfo damageEffect)
        {
            var existing = ActiveEffects;

            var count = damageEffect.RoundOff - damageEffect.RoundOn;
            var config = damageEffect.Config;
            if (existing.TryGetValue(config.Effect, out _))
                existing[config.Effect] = count;
            else
                existing.Add(config.Effect, count);

            ActiveEffects = existing;

            return this;
        }

        internal EffectsComp UseEffect(DamageEffect effect, out bool used)
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

        internal EffectsComp ResetEffects()
        {
            var existing = ActiveEffects;
            existing.Clear();

            ActiveEffects = existing;

            return this;
        }

    }

}


