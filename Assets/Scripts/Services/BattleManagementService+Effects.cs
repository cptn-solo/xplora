using Assets.Scripts.UI.Data;
using System;
using System.Linq;

namespace Assets.Scripts.Services
{
    public partial class BattleManagementService // Effects
    {
        private void EnqueueEffectToRounds(DamageEffectInfo damageEffect, Hero target)
        {
            var affected = battle.RoundsQueue
                .Where(x =>
                    x.Round <= damageEffect.RoundOff &&
                    x.Round > damageEffect.RoundOn);

            foreach (var round in affected)
            {
                for (int i = 0; i < round.QueuedHeroes.Count; i++)
                {
                    var slot = round.QueuedHeroes[i];
                    if (slot.HeroId == target.Id)
                    {
                        slot = slot.AddEffect(damageEffect);
                        round.QueuedHeroes[i] = slot;
                    }
                }
            }            
        }

        private void ApplyQueuedEffects(BattleTurnInfo turnInfo, out Hero attacker, out BattleTurnInfo? effectsInfo)
        {
            if (turnInfo.AttackerEffects.Length == 0)
            {
                attacker = turnInfo.Attacker;
                effectsInfo = null;
                return;
            }

            var effectDamage = 0;
            foreach (var eff in turnInfo.AttackerEffects)
                effectDamage += DamageEffectInfo.Draft(eff).ExtraDamage;

            attacker = turnInfo.Attacker.UpdateHealthCurrent(effectDamage, out int aDisplay, out int aCurrent);

            // intermediate turn info, no round turn override to preserve pre-calculated target:
            effectsInfo = BattleTurnInfo.Create(CurrentTurn, attacker,
                effectDamage, turnInfo.AttackerEffects);
            effectsInfo = effectsInfo?.SetState(TurnState.TurnEffects);
        }
    }
}