using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.UI.Data
{

    public struct BattleInfo
    {
        private BattleState state;
        private BattleTurnInfo currentTurn;
        private List<BattleRoundInfo> roundsQueue;

        private Team playerTeam;
        private Team enemyTeam;

        public List<Hero> PlayerHeroes { get; private set; }
        public List<Hero> EnemyHeroes { get; private set; }

        private int winnerTeamId;

        public BattleState State => state;

        public Team PlayerTeam => playerTeam;
        public Team EnemyTeam => enemyTeam;

        public BattleTurnInfo CurrentTurn => currentTurn;
        public BattleRoundInfo CurrentRound =>
            roundsQueue != null && roundsQueue.Count > 0 ? roundsQueue[0] : default;
        public BattleRoundInfo NextRound =>
            roundsQueue != null && roundsQueue.Count > 1 ? roundsQueue[1] : default;

        public List<BattleRoundInfo> RoundsQueue => roundsQueue;
        public int WinnerTeamId => winnerTeamId;

        public bool Auto { get; internal set; }
        public RoundSlotInfo[] QueuedHeroes {
            get {
                var combined = RoundsQueue.SelectMany(x => x.QueuedHeroes);
                return combined.ToArray();
            }
        }

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

            battle.roundsQueue = new();
            battle.currentTurn = BattleTurnInfo.Create(-1, Hero.Default, Hero.Default);

            battle.playerTeam = playerTeam;
            battle.PlayerHeroes = new();

            battle.enemyTeam = enemyTeam;
            battle.EnemyHeroes = new();

            battle.winnerTeamId = -1;

            return battle;
        }

        internal void SetState(BattleState state)
        {
            this.state = state;
        }
        internal void SetRoundState(RoundState state)
        {
            var currentRound = CurrentRound;
            currentRound.SetState(state);
            RoundsQueue[0] = currentRound;
        }

        internal void SetTurnState(TurnState state)
        {
            currentTurn.SetState(state);
        }
        internal void SetTurnInfo(BattleTurnInfo info)
        {
            currentTurn = info;
        }

        internal void SetWinnerTeamId(int teamId)
        {
            winnerTeamId = teamId;
        }

        internal void EnqueueRound(BattleRoundInfo battleRoundInfo)
        {
            roundsQueue.Add(battleRoundInfo);
        }

        internal void SetHeroes(Hero[] playerHeroes, Hero[] enemyHeroes)
        {
            PlayerHeroes.Clear();
            PlayerHeroes.AddRange(playerHeroes);

            EnemyHeroes.Clear();
            EnemyHeroes.AddRange(enemyHeroes);
        }

        private void RemoveHero(Hero target)
        {
            if (target.TeamId == PlayerTeam.Id)
                PlayerHeroes.Remove(
                    PlayerHeroes[PlayerHeroes.FindIndex(x => x.Id == target.Id)]);
            else
                EnemyHeroes.Remove(
                    EnemyHeroes[EnemyHeroes.FindIndex(x => x.Id == target.Id)]);
        }

        private void ReplaceHero(Hero target)
        {
            if (target.TeamId == PlayerTeam.Id)
                PlayerHeroes[PlayerHeroes
                    .FindIndex(x => x.Id == target.Id)] = target;
            else
                EnemyHeroes[EnemyHeroes
                    .FindIndex(x => x.Id == target.Id)] = target;
        }

        internal void UpdateHero(Hero target)
        {
            if (target.HealthCurrent <= 0)
            {
                RemoveHero(target);

                for (int i = 0; i < RoundsQueue.Count; i++)
                {
                    var roundInfo = RoundsQueue[i];
                    roundInfo.DequeueHero(target);
                    RoundsQueue[i] = roundInfo;
                }
            }
            else
            {
                ReplaceHero(target);
            }
        }
    }
}