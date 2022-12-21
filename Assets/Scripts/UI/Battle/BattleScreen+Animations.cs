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
        private readonly List<VisualDelegate> turnStageVisuals = new();

        private bool turnProcessingQueueActive = false;
        private bool turnVisualsProcessingActive = false;

        private Coroutine turnProcessingCoroutine = null;
        private Coroutine turnVisualsProcessingCoroutine = null;


        private void ResetTurnProcessingQueue()
        {
            turnProcessingQueue.Clear();
            turnStageVisuals.Clear();

            turnProcessingQueueActive = false;
            if (turnProcessingCoroutine != null)
                StopCoroutine(turnProcessingCoroutine);

            turnVisualsProcessingActive = false;
            if (turnVisualsProcessingCoroutine != null)
                StopCoroutine(turnVisualsProcessingCoroutine);

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

        private void EnqueueTurnAnimation(DoStage dlg, float? sec = null)
        {
            turnStageVisuals.Add(new(dlg, sec));

            if (!turnVisualsProcessingActive)
            {
                turnVisualsProcessingActive = true;
                turnVisualsProcessingCoroutine = StartCoroutine(ProcessTurnAnimationsQueue());
            }
        }
        private IEnumerator ProcessTurnAnimationsQueue()
        {
            while (turnVisualsProcessingActive)
            {
                if (turnStageVisuals.Count > 0)
                {
                    var visual = turnStageVisuals.First();
                    turnStageVisuals.RemoveAt(0);

                    while (battleManager.PlayMode == BattleMode.StepMode)
                        yield return null;

                    visual.Callback();
                    
                    yield return visual.Waiter;
                }
                
                yield return null;
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

                    ScheduleTurnStageAnimations(stage);
                }
                yield return null;
            }
        }
        private void ScheduleTurnStageAnimations(BattleTurnInfo info)
        {
            var attackerPos =
                info.Attacker.TeamId == libraryManager.PlayerTeam.Id ?
                playerBattleGround.position : enemyBattleGround.position;
            var targetPos =
                info.State == TurnState.TurnSkipped ||
                info.State == TurnState.TurnEffects ? default : 
                info.Target.TeamId == libraryManager.PlayerTeam.Id ?
                playerBattleGround.position : enemyBattleGround.position;

            var attackerRM = RaidMemberForHero(info.Attacker);
            var targetRM = 
                info.State == TurnState.TurnSkipped ||
                info.State == TurnState.TurnEffects ? null : 
                RaidMemberForHero(info.Target);

            switch (info.State)
            {
                case TurnState.TurnPrepared:

                    EnqueueTurnAnimation(() => {
                        // move both cards
                        var move = 1.0f;
                        attackerRM.HeroAnimation.Run(move, attackerPos);
                        targetRM.HeroAnimation.Run(move, targetPos);
                    }, 1.3f);
                    
                    break;

                case TurnState.TurnInProgress:
                    
                    EnqueueTurnAnimation(() => {
                        attackerRM.HeroAnimation.Attack(info.Attacker.Ranged);
                        audioService.Play(SFX.Named(info.Attacker.SndAttack));
                    }, 1f);

                    if (info.Dodged)
                    {
                        EnqueueTurnAnimation(() => {
                            targetRM.HeroAnimation.SetOverlayInfo(TurnStageInfo.Dodged);
                            audioService.Play(SFX.Named(info.Target.SndDodged));
                        }, 1f);
                    }
                    else
                    {
                        EnqueueTurnAnimation(() => {
                            targetRM.HeroAnimation.Hit(false);
                        });

                        if (info.Pierced)
                            EnqueueTurnAnimation(() => {
                                targetRM.HeroAnimation.SetOverlayInfo(TurnStageInfo.Pierced(info.Damage));
                                audioService.Play(SFX.Named(info.Target.SndPierced));
                            }, 1f);

                        if (info.Damage > 0)
                            EnqueueTurnAnimation(() => {
                                if (info.Critical)
                                {
                                    targetRM.HeroAnimation.SetOverlayInfo(TurnStageInfo.Critical(info.Damage));
                                    audioService.Play(SFX.Named(info.Target.SndCritHit));
                                }
                                else
                                {
                                    targetRM.HeroAnimation.SetOverlayInfo(TurnStageInfo.JustDamage(info.Damage));
                                    audioService.Play(SFX.Named(info.Target.SndHit));
                                }
                            }, 1f);

                        if (info.TargetEffects.Length > 0)
                            EnqueueEffects(info.TargetEffects, targetRM, info.ExtraDamage);

                        if (info.Lethal)
                            EnqueueTurnAnimation(() => {
                                targetRM.HeroAnimation.Hit(true);
                                audioService.Play(SFX.Named(info.Target.SndDied));
                            }, 1f);
                    }

                    EnqueueTurnAnimation(() => { }, 1f);
                    EnqueueTurnAnimation(() => targetRM.Hero = info.Target);

                    break;

                case TurnState.TurnEffects:
                    
                    var lethal = info.Attacker.HealthCurrent <= 0;

                    if (info.AttackerEffects.Length > 0)
                        EnqueueEffects(info.AttackerEffects, attackerRM, info.Damage);
                    else if (info.Damage > 0)
                        EnqueueTurnAnimation(() => {
                            attackerRM.HeroAnimation.Hit(false);
                            audioService.Play(SFX.Named(info.Attacker.SndHit));
                        }, 1f);

                    if (lethal)
                        EnqueueTurnAnimation(() => {
                            attackerRM.HeroAnimation.Hit(true);
                            audioService.Play(SFX.Named(info.Attacker.SndDied));
                        }, 1f);

                    EnqueueTurnAnimation(() => attackerRM.Hero = info.Attacker);
                    
                    break;

                case TurnState.TurnSkipped:
                    
                    EnqueueTurnAnimation(() => {
                        // move only attacker card to show effects (if any)
                        var move = 1.0f;
                        attackerRM.HeroAnimation.Run(move, attackerPos);
                    }, .3f);
                    
                    break;

                case TurnState.TurnCompleted:

                    EnqueueTurnAnimation(() => {
                        if (targetRM != null && info.Target.HealthCurrent <= 0)
                            targetRM.Hero = Hero.Default;

                        if (attackerRM != null && info.Attacker.HealthCurrent <= 0)
                            attackerRM.Hero = Hero.Default;

                        // move cards back or remove dead ones from the field.
                        if (attackerRM != null)
                            attackerRM.HeroAnimation.MoveSpriteBack();

                        if (targetRM != null)
                            targetRM.HeroAnimation.MoveSpriteBack();
                    }, .3f);

                    EnqueueTurnAnimation(() => {
                        turnProcessingQueueActive = false;
                        battleManager.SetTurnProcessed(info);
                    });
                    
                    break;
                
                case TurnState.NoTargets:

                    EnqueueTurnAnimation(() => {
                        turnProcessingQueueActive = false;
                        battleManager.SetTurnProcessed(info);
                    });

                    break;

                default:
                    break;
            }            
        }

        private void EnqueueEffects(DamageEffect[] effects, RaidMember rm, int extraDamage = 0)
        {
            foreach (var effect in effects)
            {
                EnqueueTurnAnimation (() => {
                    rm.HeroAnimation.SetEffects(new DamageEffect[] { effect });
                    rm.HeroAnimation.SetOverlayInfo(TurnStageInfo.EffectDamage(effect, extraDamage));
                    rm.HeroAnimation.Hit(false);
                    var sfxName = effect switch
                    {
                        DamageEffect.Bleeding => rm.Hero.SndBleeding,
                        DamageEffect.Burning => rm.Hero.SndBurning,
                        DamageEffect.Frozing => rm.Hero.SndFreezed,
                        DamageEffect.Stunned => rm.Hero.SndStunned,
                        _ => ""
                    };

                    if (sfxName != "")
                        audioService.Play(SFX.Named(sfxName));
                }, 1f);
            }
        }
    }
}