using Assets.Scripts.UI.Data;

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
                    UpdateView();
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
                        ResetTurnProcessingQueue();
                        EnqueueOrAuto(turnInfo);
                    }
                    break;
                case TurnState.TurnPrepared:
                case TurnState.TurnInProgress:
                case TurnState.TurnCompleted:
                case TurnState.NoTargets:
                    {
                        EnqueueOrAuto(turnInfo);
                    }
                    break;
                case TurnState.TurnProcessed:
                    break;
                default:
                    break;
            }

            UpdateActionButtons();

        }

        private void EnqueueOrAuto(BattleTurnInfo turnInfo)
        {
            if (battleManager.CurrentBattle.Auto &&
                (turnInfo.State == TurnState.TurnCompleted ||
                    turnInfo.State == TurnState.NoTargets))
                battleManager.SetTurnProcessed(turnInfo);
            else
                EnqueueTurnProcessingStage(turnInfo);
        }
    }
}