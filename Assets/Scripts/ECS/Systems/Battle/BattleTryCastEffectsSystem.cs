using Assets.Scripts.Data;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleTryCastEffectsSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            throw new System.NotImplementedException();
        }

        private bool TryCast(Hero attacker, Hero target, int roundOn, out DamageEffectInfo info,
            bool disableRNGToggle)
        {
            DamageEffectConfig config = libraryManager.DamageTypesLibrary
                .EffectForDamageType(attacker.DamageType);

            info = default;

            if (config.Effect == DamageEffect.NA)
                return false;

            if (!disableRNGToggle)
            {

                if (!config.ChanceRate.RatedRandomBool()) return false;

                switch (config.Effect)
                {
                    case DamageEffect.Stunned:
                        if (target.RandomResistStun) return false;
                        break;
                    case DamageEffect.Bleeding:
                        if (target.RandomResistBleeding) return false;
                        break;
                    case DamageEffect.Pierced:
                        if (target.RandomResistPierced) return false;
                        break;
                    case DamageEffect.Burning:
                        if (target.RandomResistBurning) return false;
                        break;
                    case DamageEffect.Frozing:
                        if (target.RandomResistFrozing) return false;
                        break;
                    default:
                        return false;
                }
            }
            DamageEffectInfo draft = DamageEffectInfo.Draft(config);

            draft = draft.SetDuration(roundOn, roundOn + config.TurnsActive);

            info = draft;

            return true;
        }

    }
}
