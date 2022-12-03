namespace Assets.Scripts.UI.Data
{
    public enum BattleState
    {
        NA = 0,
        Created = 100,
        PrepareTeams = 200,  // prepare team
        TeamsPrepared = 300, // ready for enqueue
        PrepareRound = 400,  // enqueue heroes
        RoundPrepared = 500, // ready for turn
        PrepareTurn = 600,   // define attacker and target
        TurnPrepared = 700,  // ready for attack
        TurnInProgress = 800, // performing attack (waiting in manual battle mode)
        TurnCompleted = 900,  // aftermath, next possible states are either 600 or 1000+
        RoundCompleted = 1000, // queue empty
        NoTargets = 1100,      // OR no targets
        Completed = 1200,
    }
}