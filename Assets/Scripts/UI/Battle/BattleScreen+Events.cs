using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using System.Linq;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Events
    {
        private void BattleManager_OnBattleEvent(BattleInfo battleInfo)
        {
            switch (battleInfo.State)
            {
                case BattleState.Created:

                    
                    break;

                case BattleState.BattleStarted:
                    
                    audioService.Play(SFX.MainTheme);
                    
                    break;

                case BattleState.Completed:
                    
                    audioService.Stop();
                                        
                    if (battleManager.CurrentBattle.WinnerTeamId == libraryManager.PlayerTeam.Id)
                        audioService.Play(CommonSoundEvent.BattleWon.SoundForEvent());
                    else if (battleManager.CurrentBattle.WinnerTeamId == libraryManager.EnemyTeam.Id)
                        audioService.Play(CommonSoundEvent.BattleLost.SoundForEvent());                    

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
                case RoundState.RoundPrepared:
                case RoundState.RoundCompleted:
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
                        //EnqueueOrAuto(turnInfo);
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
            if (battleManager.PlayMode == BattleMode.Autoplay ||
                battleManager.PlayMode == BattleMode.StepMode)
                EnqueueTurnProcessingStage(turnInfo);
        }
    }
}