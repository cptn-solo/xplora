using System.Collections.Generic;

namespace Assets.Scripts.UI.Data
{

    public struct BattleInfo
    {
        private BattleState state;
        private BattleTurnInfo currentTurn;
        private BattleRoundInfo currentRound;
        
        private List<BattleTurnInfo> prevTurns;

        private Team playerTeam;
        private Team enemyTeam;

        private int winnerTeamId;

        public BattleState State => state;

        public Team PlayerTeam => playerTeam;
        public Team EnemyTeam => enemyTeam;

        public BattleTurnInfo CurrentTurn => currentTurn;
        public BattleRoundInfo CurrentRound => currentRound;
        public int WinnerTeamId => winnerTeamId;

        public List<BattleTurnInfo> BattleLog => prevTurns;

        public bool Auto { get; internal set; }

        public override string ToString()
        {
            var full = $"Битва: " +
                $"{State}, " +
                $"{PlayerTeam} vs {EnemyTeam}, " +
                $"ход: {CurrentTurn.Turn}";
            var completed = $"Битва: " +
                $"{State}, " +
                $"{PlayerTeam} vs {EnemyTeam}, " +
                $"победила команда: {WinnerTeamId}";
            return State switch
            { 
                BattleState.BattleStarted => full,
                BattleState.Completed => completed,
                _ => $"Битва: {State}"
            };
        }

        internal static BattleInfo Create(Team playerTeam, Team enemyTeam)
        {
            BattleInfo battle = default;
            
            battle.state = BattleState.Created;

            battle.currentTurn = BattleTurnInfo.Create(-1, Hero.Default, Hero.Default);
            battle.currentRound = BattleRoundInfo.Create();

            battle.playerTeam = playerTeam;
            battle.enemyTeam = enemyTeam;

            battle.winnerTeamId = -1;
            battle.prevTurns = new();

            return battle;
        }

        internal void SetState(BattleState state)
        {
            this.state = state;
        }
        internal void SetRoundState(RoundState state)
        {
            currentRound.SetState(state);                
        }
        internal void SetRoundInfo(BattleRoundInfo info)
        { 
            currentRound = info;
        }
        internal void SetTurnState(TurnState state)
        {
            currentTurn.SetState(state);
        }
        internal void SetTurnInfo(BattleTurnInfo info)
        {
            prevTurns.Add(currentTurn);
            currentTurn = info;
        }

        internal void SetWinnerTeamId(int teamId)
        {
            this.winnerTeamId = teamId;
        }
    }
}