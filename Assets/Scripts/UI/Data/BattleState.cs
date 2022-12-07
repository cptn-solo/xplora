namespace Assets.Scripts.UI.Data
{
    public enum BattleState
    {
        NA = 0,
        Created = 100,
        PrepareTeams = 200,  // prepare team
        TeamsPrepared = 300, // ready for enqueue
        BattleStarted = 350, // 1st round will begin now
        BattleInProgress = 400, // => RoundState
        Completed = 1200,
    }

    public enum RoundState
    {
        NA = 0,
        PrepareRound = 400,  // enqueue heroes
        RoundPrepared = 500, // ready for turn
        RoundInProgress = 600, // => TurnState
        RoundCompleted = 1000, // queue empty
    }

    public enum TurnState
    { 
        NA = 0,
        PrepareTurn = 600,   // define attacker and target
        TurnPrepared = 700,  // ready for attack
        TurnInProgress = 800, // performing attack (waiting in manual battle mode)
        TurnCompleted = 900,  // aftermath, next possible states are either 600 or 1000+
        NoTargets = 1100,      // OR no targets
        TurnProcessed = 1200, // waiting for being processed by UI
    }

}