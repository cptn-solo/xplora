using Assets.Scripts.Data;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public partial class BattleManagementService // Attack
    {

        private void ProcessAttack(BattleTurnInfo turnInfo, Hero attacker,
            out Hero target, out BattleTurnInfo resultInfo)
        {
            target = turnInfo.Target;// will be replaced with round target below

            // attack:
            var accurate = prefs.DisableRNGToggle || attacker.RandomAccuracy;

            // defence:
            var dodged = !prefs.DisableRNGToggle && target.RandomDodge;

            var criticalDamage = false;

            var pierced = false;
            var targetEffects = new DamageEffect[] { };
            int extraDamage = 0;

            int damage;
            if (!accurate || dodged)
            {
                damage = 0;
            }
            else
            {
                var shield = target.DefenceRate;

                if (TryCast(attacker, target, battle.CurrentRound.Round, out var damageEffect,
                    prefs.DisableRNGToggle))
                {
                    if (damageEffect.Config.Effect == DamageEffect.Pierced)
                    {
                        pierced = true;
                        shield = (int)(damageEffect.Config.ShieldUseFactor / 100f * shield);
                    }
                    else
                    {
                        targetEffects = targetEffects.Concat(new DamageEffect[]{
                            damageEffect.Config.Effect }).ToArray();
                        extraDamage = damageEffect.Config.ExtraDamage;
                        target = target.EnqueEffect(damageEffect);
                    }
                }

                var rawDamage = prefs.DisableRNGToggle ? attacker.DamageMax : attacker.RandomDamage;
                criticalDamage = !prefs.DisableRNGToggle && attacker.RandomCriticalHit;

                damage = rawDamage;
                damage *= criticalDamage ? 2 : 1;
                damage -= (int)(damage * shield / 100f);
                damage = Mathf.Max(0, damage);
            }

            target = target.UpdateHealthCurrent(damage+extraDamage, out int display, out int current);

            resultInfo = BattleTurnInfo.Create(CurrentTurn, attacker, target,
                damage, null, targetEffects);
            resultInfo.Critical = criticalDamage;
            resultInfo.Dodged = dodged;
            resultInfo.Pierced = pierced;
            resultInfo.Lethal = current <= 0;
            resultInfo.ExtraDamage = extraDamage;
        }

    }
}