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
                    
                    if (battleManager.CurrentBattle.WinnerTeamId == libraryManager.PlayerTeam.Id)
                        audioService.Play(SFX.Enumed(CommonSoundEvent.BattleWon));
                    else if (battleManager.CurrentBattle.WinnerTeamId == libraryManager.EnemyTeam.Id)
                        audioService.Play(SFX.Enumed(CommonSoundEvent.BattleLost));                    

                    UpdateView();
                    break;
                default:
                    break;
            }

            UpdateActionButtons();

        }

        private void BattleManager_OnRoundEvent(BattleRoundInfo roundInfo, BattleInfo battleInfo)
        {
            switch (roundInfo.State)
            {
                case RoundState.NA:
                    break;
                case RoundState.PrepareRound:
                    break;
                case RoundState.RoundPrepared:
                    battleQueue.LayoutHeroes(battleInfo.QueuedHeroes);
                    break;
                case RoundState.RoundInProgress:
                    battleQueue.LayoutHeroes(battleInfo.QueuedHeroes);
                    break;
                case RoundState.RoundCompleted:
                    battleQueue.LayoutHeroes(battleInfo.QueuedHeroes);
                    break;
                default:
                    break;
            }
            
            UpdateActionButtons();
        }

        private void BattleManager_OnTurnEvent(BattleTurnInfo turnInfo, BattleRoundInfo roundInfo, BattleInfo battleInfo)
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
                case TurnState.TurnSkipped:
                case TurnState.TurnEffects:
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
            if (battleManager.CurrentBattle.Auto)
            {
                if (turnInfo.State switch
                {
                    TurnState.TurnCompleted => true,
                    TurnState.TurnSkipped => true,
                    TurnState.NoTargets => true,
                    _ => false
                })
                    battleManager.SetTurnProcessed(turnInfo);
            }
            else
            {
                EnqueueTurnProcessingStage(turnInfo);
            }
        }
    }
}