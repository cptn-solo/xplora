using Assets.Scripts.UI.Data;
using System;
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
        private void ResetTurnProcessingQueue(BattleTurnInfo turnInfo)
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
                info.Target.TeamId == libraryManager.PlayerTeam.Id ?
                playerBattleGround : enemyBattleGround;

            var attakerRM = RaidMemberForHero(info.Attacker);
            var targetRM = RaidMemberForHero(info.Target);

            if (info.State == TurnState.TurnPrepared)
            {
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
                //attakerRM.HeroAnimation.transform.position = attackerAnimation.position;
                //targetRM.HeroAnimation.transform.position = targetAnimation.position;
                
                yield return new WaitForSeconds(.3f);

            }
            else if (info.State == TurnState.TurnInProgress)
            {
                // animate attack and hit
                //Debug.Break();
                attakerRM.HeroAnimation.Attack(info.Attacker.Ranged);
                audioService.Play(SFX.Named(info.Attacker.SndAttack));

                yield return new WaitForSeconds(.8f);

                targetRM.HeroAnimation.Hit(info.Lethal);
                audioService.Play(SFX.Named(info.Lethal ? info.Target.SndDied : info.Target.SndHit));

                yield return new WaitForSeconds(1f);

                // ??
                targetRM.Hero = info.Target;
            }
            else if (info.State == TurnState.TurnCompleted)
            {
                if (info.Lethal)
                {
                    targetRM.HeroAnimation.transform.localPosition = Vector3.zero;
                    targetRM.Hero = Hero.Default;
                }                    
                else
                {
                    targetRM.HeroAnimation.transform.localPosition = Vector3.zero;
                }

                // move cards back or remove dead ones from the field
                attakerRM.HeroAnimation.transform.localPosition = Vector3.zero;
                
                yield return new WaitForSeconds(.3f);

            }

            battleQueue.UpdateHero(info.Target);
            
            turnStageProcessingActive = false;
        }
    }
}