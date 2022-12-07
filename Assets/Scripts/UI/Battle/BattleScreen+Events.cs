using Assets.Scripts.UI.Data;
using System;
using Zenject;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Events
    {
        private void BattleManager_OnBattleEvent(BattleInfo battleInfo)
        {
            switch (battleInfo.State)
            {
                case BattleState.NA:
                    break;
                case BattleState.Created:
                    {
                        ShowTeamInventory(libraryManager.PlayerTeam);
                        ShowTeamInventory(libraryManager.EnemyTeam);
                    }
                    break;
                case BattleState.PrepareTeams:
                    break;
                case BattleState.TeamsPrepared:
                    break;
                case BattleState.BattleStarted:
                    audioService.Play(SFX.MainTheme);
                    break;
                case BattleState.BattleInProgress:
                    break;
                case BattleState.Completed:
                    audioService.Stop();
                    break;
                default:
                    break;
            }

            UpdateActionButtons();

        }

        private void BattleManager_OnRoundEvent(BattleRoundInfo roundInfo)
        {
            switch (roundInfo.State)
            {
                case RoundState.NA:
                    break;
                case RoundState.PrepareRound:
                    break;
                case RoundState.RoundPrepared:
                    battleQueue.LayoutHeroes(roundInfo.QueuedHeroes.ToArray());
                    break;
                case RoundState.RoundInProgress:
                    battleQueue.LayoutHeroes(roundInfo.QueuedHeroes.ToArray());
                    break;
                case RoundState.RoundCompleted:
                    battleQueue.LayoutHeroes(roundInfo.QueuedHeroes.ToArray());
                    break;
                default:
                    break;
            }
            
            UpdateActionButtons();
        }

        private void BattleManager_OnTurnEvent(BattleTurnInfo turnInfo)
        {
            switch (turnInfo.State)
            {
                case TurnState.PrepareTurn:
                    {
                        ResetTurnProcessingQueue(turnInfo);
                        EnqueueTurnProcessingStage(turnInfo);
                    }
                    break;
                case TurnState.TurnPrepared:
                case TurnState.TurnInProgress:
                case TurnState.TurnCompleted:
                case TurnState.NoTargets:
                    {
                        EnqueueTurnProcessingStage(turnInfo);
                    }
                    break;
                case TurnState.TurnProcessed:
                    break;
                default:
                    break;
            }
            
            UpdateActionButtons();

        }
    }
}