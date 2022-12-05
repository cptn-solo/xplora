﻿using System;

namespace Assets.Scripts.UI.Data
{

    public struct BattleInfo
    {
        private BattleState state;
        private BattleTurnInfo currentTurn;
        private BattleRoundInfo currentRound;

        private Team playerTeam;
        private Team enemyTeam;

        private int winnerTeamId;

        public BattleState State => state;

        public Team PlayerTeam => playerTeam;
        public Team EnemyTeam => enemyTeam;

        public BattleTurnInfo CurrentTurn => currentTurn;
        public BattleRoundInfo CurrentRound => currentRound;
        public int WinnerTeamId => winnerTeamId;

        internal static BattleInfo Create(Team playerTeam, Team enemyTeam)
        {
            BattleInfo battle = default;
            
            battle.state = BattleState.Created;

            battle.currentTurn = BattleTurnInfo.Create(-1, Hero.Default, Hero.Default);
            battle.currentRound = BattleRoundInfo.Create();

            battle.playerTeam = playerTeam;
            battle.enemyTeam = enemyTeam;

            battle.winnerTeamId = -1;


            return battle;
        }

        internal void SetState(BattleState state)
        {
            this.state = state;
        }
    }
}