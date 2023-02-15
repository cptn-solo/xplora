using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using System.Linq;

namespace Assets.Scripts.Services
{
    public partial class BattleManagementService // Effects
    {
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
        private void ApplyQueuedEffects(BattleTurnInfo turnInfo, out Hero attacker, out BattleTurnInfo? effectsInfo)
        {
            attacker = turnInfo.Attacker;
            if (attacker.ActiveEffects.Count == 0)
            {
                effectsInfo = null;
                return;
            }
            
            var effs = attacker.ActiveEffects.Keys.ToArray(); // will be used to flash used effects ===>

            var effectDamage = 0;
            foreach (var eff in effs)
            {
                effectDamage += libraryManager.DamageTypesLibrary
                    .EffectForDamageEffect(eff).ExtraDamage;
                attacker = attacker.UseEffect(eff, out var used);
            }

            attacker = turnInfo.Attacker.UpdateHealthCurrent(effectDamage, out int aDisplay, out int aCurrent);

            // intermediate turn info, no round turn override to preserve pre-calculated target:
            effectsInfo = BattleTurnInfo.Create(CurrentTurn, attacker,
                effectDamage, effs); // <===
            effectsInfo = effectsInfo?.SetState(TurnState.TurnEffects);
        }
    }
}