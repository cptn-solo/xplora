using Assets.Scripts.UI.Battle;

namespace Assets.Scripts.UI.Data
{

    public struct DamageEffectInfo
    {
        public DamageEffect Effect { get; private set; }
        public bool TurnSkipped { get; private set; }
        public int TurnsActive { get; private set; }
        public int ExtraDamage { get; private set; }
        public int ShieldUseFactor { get; private set; } // % of target's shield used to deflect extra damage
        public int RoundOn { get; private set; }
        public int RoundOff { get; private set; }

        public override string ToString()
        {
            return $"{Effect}:{ExtraDamage}/{TurnsActive}";
        }

        public static DamageEffectInfo Draft(DamageEffect effect)
        {
            DamageEffectInfo info = default;
            
            info.Effect = effect;
            switch (effect)
            {
                case DamageEffect.NA:
                    break;
                case DamageEffect.Stunned:
                    {
                        info.TurnSkipped = true;
                        info.TurnsActive = 1;
                        info.ExtraDamage = 0;
                        info.ShieldUseFactor = 100;
                    }
                    break;
                case DamageEffect.Bleeding:
                    {
                        info.TurnSkipped = false;
                        info.TurnsActive = 3;
                        info.ExtraDamage = 3;
                        info.ShieldUseFactor = 100;
                    }
                    break;
                case DamageEffect.Pierced:
                    {
                        info.TurnSkipped = false;
                        info.TurnsActive = 0;
                        info.ExtraDamage = 0;
                        info.ShieldUseFactor = 0;
                    }
                    break;
                case DamageEffect.Burning:
                    {
                        info.TurnSkipped = false;
                        info.TurnsActive = 2;
                        info.ExtraDamage = 4;
                        info.ShieldUseFactor = 100;
                    }
                    break;
                case DamageEffect.Frozing:
                    {
                        info.TurnSkipped = true;
                        info.TurnsActive = 1;
                        info.ExtraDamage = 2;
                        info.ShieldUseFactor = 100;
                    }
                    break;
                default:
                    break;
            }
            return info;
        }

        public static bool TryCast(Hero attacker, Hero target, int roundOn, out DamageEffectInfo info, 
            bool disableRNGToggle)
        {            
            DamageEffect effect = attacker.DamageType switch
            {
                DamageType.Force => DamageEffect.Stunned,
                DamageType.Cut => DamageEffect.Bleeding,
                DamageType.Pierce => DamageEffect.Pierced,
                DamageType.Burn => DamageEffect.Burning,
                DamageType.Frost => DamageEffect.Frozing,
                _ => DamageEffect.NA
            };
            info = default;

            if (effect == DamageEffect.NA)
                return false;

            if (!disableRNGToggle)
            {
                var damageTypeInfo = new DamageTypeInfo();
                switch (effect)
                {
                    case DamageEffect.Stunned:
                        if (!damageTypeInfo.RandomEffectStun ||
                            target.RandomResistStun) return false;
                        break;
                    case DamageEffect.Bleeding:
                        if (!damageTypeInfo.RandomEffectBleed ||
                            target.RandomResistBleeding) return false;
                        break;
                    case DamageEffect.Pierced:
                        if (!damageTypeInfo.RandomEffectPierce ||
                            target.RandomResistPierced) return false;
                        break;
                    case DamageEffect.Burning:
                        if (!damageTypeInfo.RandomEffectBurn ||
                            target.RandomResistBurning) return false;
                        break;
                    case DamageEffect.Frozing:
                        if (!damageTypeInfo.RandomEffectFrost ||
                            target.RandomResistFrozing) return false;
                        break;
                    default:
                        return false;
                }
            }
            DamageEffectInfo draft = Draft(effect);

            draft.RoundOn = roundOn;
            draft.RoundOff = roundOn + draft.TurnsActive;

            info = draft;

            return true;
        }
    }
}