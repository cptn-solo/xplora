using System.Linq;
using Assets.Scripts.Data;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleApplyQueuedEffectsSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            throw new System.NotImplementedException();
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
