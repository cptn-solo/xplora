namespace Assets.Scripts.UI.Data
{
    public struct DamageEffectInfo
    {
        public Hero Hero { get; private set; }
        public DamageEffect Effect { get; private set; }
        public bool TurnSkipped { get; private set; }
        public int TurnsActive { get; private set; }
        public int ExtraDamage { get; private set; }
        public int ShieldUseFactor { get; private set; } // % of target's shield used to deflect extra damage
        public int RoundOn { get; private set; }
        public int RoundOff { get; private set; }

        public static bool TryCast(Hero attacker, Hero target, int roundOn, out DamageEffectInfo info)
        {
            info = default;
            info.Effect = DamageEffect.NA;
            
            DamageEffect effect = attacker.DamageType switch
            {
                DamageType.Force => DamageEffect.Stunned,
                DamageType.Cut => DamageEffect.Bleeding,
                DamageType.Pierce => DamageEffect.Pierced,
                DamageType.Burn => DamageEffect.Burning,
                DamageType.Frost => DamageEffect.Frozing,
                _ => DamageEffect.NA
            };

            switch (effect)
            {
                case DamageEffect.NA:
                    return false;
                case DamageEffect.Stunned:
                    if (!attacker.RandomEffectStun ||
                        target.RandomResistStun) return false;
                    break;
                case DamageEffect.Bleeding:
                    if (!attacker.RandomEffectBleed ||
                        target.RandomResistBleeding) return false;
                    break;
                case DamageEffect.Pierced:
                    if (!attacker.RandomEffectPierce ||
                        target.RandomResistPierced) return false;
                    break;
                case DamageEffect.Burning:
                    if (!attacker.RandomEffectBurn ||
                        target.RandomResistBurning) return false;
                    break;
                case DamageEffect.Frozing:
                    if (!attacker.RandomEffectFrost ||
                        target.RandomResistFrozing) return false;
                    break;
                default:
                    break;
            }
            
            info.Hero = target;
            info.Effect = effect;
            info.RoundOn = roundOn;

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
            
            info.RoundOff = roundOn + info.TurnsActive;

            return true;
        }
    }
}