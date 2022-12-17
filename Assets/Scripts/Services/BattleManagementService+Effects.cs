using Assets.Scripts.UI.Data;
using System.Collections;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public partial class BattleManagementService // Effects
    {
        private void EnqueueEffectToRounds(DamageEffectInfo damageEffect, Hero target)
        {
            var combined = battle.RoundsQueue
                .Where(x =>
                    x.Round <= damageEffect.RoundOff &&
                    x.Round > damageEffect.RoundOn)
                .SelectMany(x => x.QueuedHeroes)
                .Where(x => x.HeroId == target.Id);

            if (combined.Count() > 0)
                foreach (var slot in combined)
                    slot.Effects.Add(damageEffect.Effect);
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