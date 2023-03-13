using Assets.Scripts.Data;
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

            battleManager.TryGetEntityViewForPackedEntity<Hero, BattleUnit>(
                info.Attacker, out var attackerRM);
            battleManager.TryGetEntityViewForPackedEntity<Hero, BattleUnit>(
                info.Target, out var targetRM);

            var attackerPos = attackerRM.transform.position;

            if (targetRM != null)
            {
                attackerPos.y = targetRM.HeroAnimation.transform.position.y;
                attackerPos.x =
                    attackerRM.transform.position.x +
                    (targetRM.transform.position.x -
                    attackerRM.transform.position.x) * .6f;
            }

            switch (info.State)
            {
                case TurnState.TurnPrepared:

                    EnqueueTurnAnimation(() => {
                        // move both cards
                        var move = 1.0f;
                        attackerRM.HeroAnimation.Run(move, attackerPos);                        
                    }, 1.3f);
                    
                    break;

                case TurnState.TurnSkipped:                    
                    
                    break;

                case TurnState.TurnEffects:


                    if (info.AttackerEffects.Length > 0)
                        EnqueueEffects(info.AttackerEffects, attackerRM, info.Damage);
                    else if (info.Damage > 0)
                        EnqueueTurnAnimation(() => {
                            attackerRM.HeroAnimation.Hit();
                            audioService.Play(SFX.Named(info.AttackerConfig.SndHit));
                        }, 1f);

                    if (info.Lethal)
                    {
                        EnqueueTurnAnimation(() => {
                            attackerRM.HeroAnimation.Death();
                            audioService.Play(SFX.Named(info.AttackerConfig.SndDied));
                        }, 1f);
                        EnqueueTurnAnimation(() =>
                        {
                            var packed = attackerRM.PackedEntity.Value;
                            battleManager.EnqueueEntityViewDestroy<Hero>(
                                packed);
                            battleManager.EnqueueEntityViewDestroy<BarsAndEffectsInfo>(
                                packed);
                        });
                    }

                    if (!info.Lethal)
                        EnqueueTurnAnimation(() =>
                            battleManager.EnqueueEntityViewUpdate<BarsAndEffectsInfo>(
                                attackerRM.PackedEntity.Value));

                    break;

                case TurnState.TurnInProgress:

                    EnqueueTurnAnimation(() => {
                        attackerRM.HeroAnimation.Attack(info.AttackerConfig.Ranged);
                        audioService.Play(SFX.Named(info.AttackerConfig.SndAttack));
                    }, 1f);

                    if (info.Dodged)
                    {
                        EnqueueTurnAnimation(() => {
                            targetRM.HeroAnimation.SetOverlayInfo(TurnStageInfo.Dodged);
                            audioService.Play(SFX.Named(info.TargetConfig.SndDodged));
                        }, 1f);
                    }
                    else
                    {
                        EnqueueTurnAnimation(() => {
                            targetRM.HeroAnimation.Hit();
                        });

                        if (info.Pierced)
                            EnqueueTurnAnimation(() => {
                                targetRM.HeroAnimation.SetOverlayInfo(TurnStageInfo.Pierced(info.Damage));
                                audioService.Play(SFX.Named(info.TargetConfig.SndPierced));
                            }, 1f);

                        if (info.Damage > 0)
                            EnqueueTurnAnimation(() => {
                                if (info.Critical)
                                {
                                    targetRM.HeroAnimation.SetOverlayInfo(TurnStageInfo.Critical(info.Damage));
                                    audioService.Play(SFX.Named(info.TargetConfig.SndCritHit));
                                }
                                else
                                {
                                    targetRM.HeroAnimation.SetOverlayInfo(TurnStageInfo.JustDamage(info.Damage));
                                    audioService.Play(SFX.Named(info.TargetConfig.SndHit));
                                }
                            }, 1f);

                        if (info.TargetEffects.Length > 0)
                            EnqueueEffects(info.TargetEffects, targetRM, info.ExtraDamage);

                        if (info.Lethal)
                            EnqueueTurnAnimation(() => {
                                targetRM.HeroAnimation.Death();
                                audioService.Play(SFX.Named(info.TargetConfig.SndDied));
                            }, 1f);
                    }

                    EnqueueTurnAnimation(() => { }, .1f);
                    EnqueueTurnAnimation(() =>
                        battleManager.EnqueueEntityViewUpdate<BarsAndEffectsInfo>(
                            targetRM.PackedEntity.Value));
                    EnqueueTurnAnimation(() => { }, 1f);

                    break;

                case TurnState.TurnCompleted:

                    EnqueueTurnAnimation(() => {
                        if (targetRM != null && info.Lethal)
                            targetRM.SetHero(null);

                        // move cards back or remove dead ones from the field.
                        if (attackerRM != null)
                        {
                            attackerRM.HeroAnimation.MoveSpriteBack();
                        }

                        if (targetRM != null)
                        {
                            targetRM.HeroAnimation.MoveSpriteBack();
                        }
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

        private void EnqueueEffects(DamageEffect[] effects, BattleUnit rm, int extraDamage = 0)
        {
            foreach (var effect in effects)
            {
                EnqueueTurnAnimation (() => {
                    rm.HeroAnimation.FlashEffect(effect);
                    rm.HeroAnimation.SetOverlayInfo(TurnStageInfo.EffectDamage(effect, extraDamage));
                    rm.HeroAnimation.Hit();
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