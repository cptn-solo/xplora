using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;
using System.Collections;

namespace Assets.Scripts.Services
{
    public partial class BattleManagementService // Router
    {
        internal IEnumerator BattleRouterCoroutine()
        {
            battleRouterCoroutineRunning = true;

            battle.SetState(BattleState.BattleInProgress);
            OnBattleEvent?.Invoke(battle);

            int winnerTeamId = -1;

            while (battle.State == BattleState.BattleInProgress)
            {
                if (winnerTeamId >= 0)
                {
                    playMode = BattleMode.NA;
                    battle.SetState(BattleState.Completed);
                    break;
                }

                StartCoroutine(PrepareNextRoundsCoroutine());

                while (battle.State == BattleState.BattleInProgress && 
                    battle.CurrentRound.State != RoundState.RoundPrepared)
                    yield return null;

                battle.SetRoundState(RoundState.RoundInProgress);

                while (battle.State == BattleState.BattleInProgress && 
                    battle.CurrentRound.State == RoundState.RoundInProgress)
                {
                    winnerTeamId = CheckForWinner();
                    if (winnerTeamId >= 0)
                    {
                        battle.SetWinnerTeamId(winnerTeamId);
                        battle.SetRoundState(RoundState.RoundCompleted);
                        break;
                    }

                    if (battle.CurrentRound.QueuedHeroes.Count == 0)
                    {
                        battle.SetRoundState(RoundState.RoundCompleted);
                        break;
                    }

                    OnRoundEvent?.Invoke(battle.CurrentRound, battle);

                    PrepareNextTurn();

                    while (battle.State == BattleState.BattleInProgress &&
                        battle.CurrentTurn.State != TurnState.TurnPrepared &&
                        battle.CurrentTurn.State != TurnState.TurnSkipped)
                        yield return null;

                    OnTurnEvent?.Invoke(battle.CurrentTurn, battle.CurrentRound, battle);

                    while (battle.State == BattleState.BattleInProgress && 
                        battle.CurrentTurn.State != TurnState.TurnCompleted)
                    {
                        if (PlayMode == BattleMode.Autoplay ||
                            PlayMode == BattleMode.Fastforward)
                            MakeTurn();
                        
                        yield return null;
                    }

                    OnTurnEvent?.Invoke(battle.CurrentTurn, battle.CurrentRound, battle);

                    while (battle.State == BattleState.BattleInProgress && 
                        battle.CurrentTurn.State != TurnState.TurnProcessed)
                    {
                        if (PlayMode == BattleMode.Fastforward)
                            SetTurnProcessed(battle.CurrentTurn);

                        yield return null;
                    }

                    OnTurnEvent?.Invoke(battle.CurrentTurn, battle.CurrentRound, battle);
                }

                OnRoundEvent?.Invoke(battle.CurrentRound, battle);
            }

            OnBattleEvent?.Invoke(battle);

            battleRouterCoroutineRunning = false;
        }

    }
}