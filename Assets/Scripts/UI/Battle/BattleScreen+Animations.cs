using Assets.Scripts.UI.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen
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
                    turnStageProcessingCoroutine = StartCoroutine(SetTurnAnimation(stage));
                    while (turnStageProcessingActive)
                        yield return null;
                }
                yield return null;

            }
        }
        private IEnumerator SetTurnAnimation(BattleTurnInfo info)
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
                // move cards to the battle ground
                attakerRM.HeroAnimation.transform.position = attackerAnimation.position;
                targetRM.HeroAnimation.transform.position = targetAnimation.position;
            }
            else if (info.State == TurnState.TurnInProgress)
            {
                // animate attack and hit
                attakerRM.HeroAnimation.Attack();

                yield return new WaitForSeconds(.5f);

                targetRM.HeroAnimation.Hit(info.Lethal);

                yield return new WaitForSeconds(1f);

                // ??
                targetRM.Hero = info.Target;
            }
            else if (info.State == TurnState.TurnCompleted)
            {
                // move cards back or remove dead ones from the field
                attakerRM.HeroAnimation.transform.localPosition =  Vector3.zero;
                targetRM.HeroAnimation.transform.localPosition = Vector3.zero;
            }

            battleQueue.UpdateHero(info.Target);
            
            turnStageProcessingActive = false;
        }
    }
}