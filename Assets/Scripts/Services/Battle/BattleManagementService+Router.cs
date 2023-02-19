using Assets.Scripts.Data;
using System.Collections;

namespace Assets.Scripts.Services
{
    public partial class BattleManagementService // Router
    {
        internal IEnumerator BattleRouterCoroutine()
        {
            battleRouterCoroutineRunning = true;

            CurrentBattle.SetState(BattleState.BattleInProgress);
            OnBattleEvent?.Invoke(CurrentBattle);

            int winnerTeamId = -1;

            while (CurrentBattle.State == BattleState.BattleInProgress)
            {
                if (winnerTeamId >= 0)
                {
                    playMode = BattleMode.NA;
                    CurrentBattle.SetState(BattleState.Completed);
                    break;
                }

                StartCoroutine(PrepareNextRoundsCoroutine());

                while (CurrentBattle.State == BattleState.BattleInProgress && 
                    CurrentRound.State != RoundState.RoundPrepared)
                    yield return null;

                CurrentRound.SetState(RoundState.RoundInProgress);

                while (CurrentBattle.State == BattleState.BattleInProgress && 
                    CurrentRound.State == RoundState.RoundInProgress)
                {
                    winnerTeamId = CheckForWinner();
                    if (winnerTeamId >= 0)
                    {
                        CurrentBattle.SetWinnerTeamId(winnerTeamId);
                        CurrentRound.SetState(RoundState.RoundCompleted);
                        break;
                    }

                    if (CurrentRound.QueuedHeroes.Count == 0)
                    {
                        CurrentRound.SetState(RoundState.RoundCompleted);
                        break;
                    }

                    OnRoundEvent?.Invoke(CurrentRound, CurrentBattle);

                    PrepareNextTurn();

                    while (CurrentBattle.State == BattleState.BattleInProgress &&
                        CurrentTurn.State != TurnState.TurnPrepared &&
                        CurrentTurn.State != TurnState.TurnSkipped)
                        yield return null;

                    OnTurnEvent?.Invoke(CurrentTurn, CurrentRound, CurrentBattle);

                    while (CurrentBattle.State == BattleState.BattleInProgress && 
                        CurrentTurn.State != TurnState.TurnCompleted)
                    {
                        if (PlayMode == BattleMode.Autoplay ||
                            PlayMode == BattleMode.Fastforward)
                            MakeTurn();
                        
                        yield return null;
                    }

                    OnTurnEvent?.Invoke(CurrentTurn, CurrentRound, CurrentBattle);

                    while (CurrentBattle.State == BattleState.BattleInProgress && 
                        CurrentTurn.State != TurnState.TurnProcessed)
                    {
                        if (PlayMode == BattleMode.Fastforward)
                            SetTurnProcessed(CurrentTurn);

                        yield return null;
                    }

                    OnTurnEvent?.Invoke(CurrentTurn, CurrentRound, CurrentBattle);
                }

                OnRoundEvent?.Invoke(CurrentRound, CurrentBattle);
            }

            OnBattleEvent?.Invoke(CurrentBattle);

            battleRouterCoroutineRunning = false;
        }

    }
}