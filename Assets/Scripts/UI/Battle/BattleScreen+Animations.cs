using Assets.Scripts.UI.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Animations
    {
        private readonly List<BattleTurnInfo> turnProcessingQueue = new();

        private bool turnProcessingQueueActive = false;
        private bool turnStageProcessingActive = false;
        private Coroutine turnProcessingCoroutine = null;
        private Coroutine turnStageProcessingCoroutine = null;
        private void ResetTurnProcessingQueue()
        {
            turnProcessingQueue.Clear();
            
            turnProcessingQueueActive = false;
            if (turnProcessingCoroutine != null)
                StopCoroutine(turnProcessingCoroutine);
            
            turnStageProcessingActive = false;
            if (turnStageProcessingCoroutine != null)
                StopCoroutine(turnStageProcessingCoroutine);

            ResetBattlefieldPositions();
        }

        private void EnqueueTurnProcessingStage(BattleTurnInfo info)
        {
            turnProcessingQueue.Add(info);

            if (!turnProcessingQueueActive)
            {
                turnProcessingQueueActive = true;
                turnProcessingCoroutine = StartCoroutine(ProcessTurnStageQueue());
            }
        }

        private IEnumerator ProcessTurnStageQueue()
        {
            while (turnProcessingQueueActive)
            {
                if (turnProcessingQueue.Count > 0)
                {
                    var stage = turnProcessingQueue.First();
                    turnProcessingQueue.RemoveAt(0);

                    turnStageProcessingActive = true;
                    turnStageProcessingCoroutine = StartCoroutine(TurnStageAnimation(stage));
                    while (turnStageProcessingActive)
                        yield return null;
                    if (stage.State == TurnState.TurnCompleted ||
                        stage.State == TurnState.NoTargets)
                    {
                        turnProcessingQueueActive = false;
                        battleManager.SetTurnProcessed(stage);
                    }
                }
                yield return null;

            }
        }
        private IEnumerator TurnStageAnimation(BattleTurnInfo info)
        {
            var attackerAnimation =
                info.Attacker.TeamId == libraryManager.PlayerTeam.Id ?
                playerBattleGround : enemyBattleGround;
            var targetAnimation =
                info.State == TurnState.TurnSkipped ||
                info.State == TurnState.TurnEffects ? null : 
                info.Target.TeamId == libraryManager.PlayerTeam.Id ?
                playerBattleGround : enemyBattleGround;

            var attakerRM = RaidMemberForHero(info.Attacker);
            var targetRM = 
                info.State == TurnState.TurnSkipped ||
                info.State == TurnState.TurnEffects ? null : 
                RaidMemberForHero(info.Target);

            if (info.State == TurnState.TurnEffects)
            {
                var lethal = info.Attacker.HealthCurrent <= 0;

                if (info.Attacker.Effects.Count > 0)
                {
                    foreach (var effect in info.Attacker.Effects)
                    {
                        attakerRM.HeroAnimation.Hit(false);
                        var sfxName = effect.Effect switch
                        {
                            DamageEffect.Bleeding => info.Attacker.SndBleeding,
                            DamageEffect.Burning => info.Attacker.SndBurning,
                            DamageEffect.Frozing => info.Attacker.SndFreezed,
                            DamageEffect.Stunned => info.Attacker.SndStunned,
                            _ => ""
                        };
                        
                        if (sfxName != "")
                            audioService.Play(SFX.Named(sfxName));
                        
                        yield return new WaitForSeconds(1f);
                    }
                } else if (info.Damage > 0)
                {
                    attakerRM.HeroAnimation.Hit(false);
                    audioService.Play(SFX.Named(info.Attacker.SndHit));
                    yield return new WaitForSeconds(1f);
                }
                if (lethal)
                {
                    attakerRM.HeroAnimation.Hit(true);
                    audioService.Play(SFX.Named(info.Attacker.SndDied));
                    yield return new WaitForSeconds(1f);
                }

                attakerRM.Hero = info.Attacker;
            }
            else if (info.State == TurnState.TurnSkipped)
            {
                // move only attacker card to show effects (if any)
                var move = 1.0f;

                attakerRM.HeroAnimation.Run(move);
                // move cards to the battle ground
                attakerRM.HeroAnimation.transform.localPosition = Vector3.zero;
                var attackerMove = attackerAnimation.position - attakerRM.HeroAnimation.transform.position;

                while (move > 0f)
                {
                    attakerRM.HeroAnimation.transform.position += attackerMove * Time.deltaTime;

                    move -= Time.deltaTime;

                    yield return null;
                }

                yield return new WaitForSeconds(.3f);
            }
            else if (info.State == TurnState.TurnPrepared)
            {
                // move both cards
                var move = 1.0f;

                attakerRM.HeroAnimation.Run(move);
                targetRM.HeroAnimation.Run(move);
                // move cards to the battle ground
                attakerRM.HeroAnimation.transform.localPosition = Vector3.zero;
                targetRM.HeroAnimation.transform.localPosition = Vector3.zero;
                var attackerMove = attackerAnimation.position - attakerRM.HeroAnimation.transform.position;
                var targetMove = targetAnimation.position - targetRM.HeroAnimation.transform.position;

                while (move > 0f)
                {
                    attakerRM.HeroAnimation.transform.position += attackerMove * Time.deltaTime;
                    targetRM.HeroAnimation.transform.position += targetMove * Time.deltaTime;
                    
                    move -= Time.deltaTime;
                    
                    yield return null;
                }
                
                yield return new WaitForSeconds(.3f);

            }
            else if (info.State == TurnState.TurnInProgress)
            {
                // animate attack and hit
                //Debug.Break();
                attakerRM.HeroAnimation.Attack(info.Attacker.Ranged);
                audioService.Play(SFX.Named(info.Attacker.SndAttack));

                yield return new WaitForSeconds(.8f);



                if (info.Dodged)
                {
                    audioService.Play(SFX.Named(info.Target.SndDodged));
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    targetRM.HeroAnimation.Hit(false);
                    if (info.Pierced)
                    {
                        audioService.Play(SFX.Named(info.Target.SndPierced));
                        yield return new WaitForSeconds(1f);
                    }
                    if (info.Damage > 0)
                    {
                        audioService.Play(SFX.Named(info.Target.SndHit));
                        yield return new WaitForSeconds(1f);
                    }
                    if (info.Critical)
                    {
                        audioService.Play(SFX.Named(info.Target.SndCritHit));
                        yield return new WaitForSeconds(1f);
                    }

                    if (info.Lethal)
                    {
                        targetRM.HeroAnimation.Hit(true);
                        audioService.Play(SFX.Named(info.Target.SndDied));
                        yield return new WaitForSeconds(1f);
                    }
                }

                yield return new WaitForSeconds(1f);

                // ??
                targetRM.Hero = info.Target;
            }
            else if (info.State == TurnState.TurnCompleted)
            {
                if (targetRM != null && info.Target.HealthCurrent <= 0)
                    targetRM.Hero = Hero.Default;

                if (attakerRM != null && info.Attacker.HealthCurrent <= 0)
                    attakerRM.Hero = Hero.Default;

                // move cards back or remove dead ones from the field.
                if (attakerRM != null)
                    attakerRM.HeroAnimation.transform.localPosition = Vector3.zero;
                
                if (targetRM != null)
                    targetRM.HeroAnimation.transform.localPosition = Vector3.zero;

                yield return new WaitForSeconds(.3f);

            }
            
            turnStageProcessingActive = false;
        }
    }
}