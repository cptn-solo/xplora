using Assets.Scripts.UI.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class BattleManagementService : MonoBehaviour
    {
        [Inject] private readonly HeroLibraryManagementService libraryManager;

        public event UnityAction<BattleInfo> OnBattleEvent;
        public event UnityAction<BattleRoundInfo> OnRoundEvent;
        public event UnityAction<BattleTurnInfo> OnTurnEvent;

        private BattleInfo battle;
        public BattleInfo CurrentBattle => battle;

        public Hero CurrentAttacker => battle.CurrentRound.CurrentAttacker;
        public int CurrentTurn => battle.CurrentTurn.Turn;
        public int QueueLength { get; internal set; }
        public bool CanPrepareRound => battle.CurrentRound.State switch
        {
            RoundState.PrepareRound => true,
            RoundState.RoundPrepared => true,
            RoundState.RoundCompleted => true,
            _ => false
        }; //  queuedHeroes.Count == 0 && AllBattleHeroes.Count() > 0;

        public bool CanBeginBattle => 
            battle.State == BattleState.TeamsPrepared &&
            battle.CurrentRound.State == RoundState.RoundPrepared;

        public bool CanBeginRound => battle.CurrentRound.State switch
        {
            RoundState.RoundPrepared => battle.State switch
            {
                BattleState.BattleStarted => true,
                BattleState.BattleInProgress => true,
                _ => false
            },
            _ => false
        }; //queuedHeroes.Count > 0 && CurrentTurn < 0
        public bool CanMakeTurn => battle.CurrentTurn.State switch
        {
            TurnState.TurnPrepared => true,
            _ => false
        }; //queuedHeroes.Count > 0;        

        public bool CanAutoPlayBattle => !battle.Auto && battle.State switch
        {
            BattleState.TeamsPrepared => true,
            BattleState.BattleInProgress => true,
            _ => false,
        };

        /// <summary>
        ///     Flag to let the battle screen know if it should reset the battle/queue
        /// </summary>
        public void ResetBattle()
        {
            libraryManager.ResetTeams();            
            battle = BattleInfo.Create(libraryManager.PlayerTeam, libraryManager.EnemyTeam);
            battle.Auto = false;

            GiveAssetsToTeams();

            battle.SetState(BattleState.PrepareTeams);
            OnBattleEvent?.Invoke(battle);

            if (libraryManager.PrepareTeamsForBattle())
            {
                battle.SetState(BattleState.TeamsPrepared);
                OnBattleEvent?.Invoke(battle);

                PrepareRound();
            }

        }

        public void MoveHero(Hero hero)
        {
            PrepareRound();
        }

        internal void PrepareRound()
        {
            var round = BattleRoundInfo.Create();
            battle.SetRoundInfo(round);
            battle.SetRoundState(RoundState.PrepareRound);
            battle.CurrentRound.ResetQueue();

            OnRoundEvent?.Invoke(battle.CurrentRound);

            var allBattleHeroes = libraryManager.Library.BattleHeroes;
            var activeHeroes = allBattleHeroes.Where(x => x.HealthCurrent > 0);
            var orderedHeroes = activeHeroes.OrderByDescending(x => x.Speed);

            Dictionary<int, List<Hero>> speedSlots = new();
            foreach (var hero in orderedHeroes)
            {
                if (speedSlots.TryGetValue(hero.Speed, out var slots))
                    slots.Add(hero);
                else
                    speedSlots[hero.Speed] = new List<Hero>() { hero };
            }

            var speedKeys = speedSlots.Keys.OrderByDescending(x => x);

            foreach (var speed in speedKeys)
            {
                var slots = speedSlots[speed];
                while (slots.Count() > 0)
                {
                    var choosenIdx = slots.Count() == 1 ? 0 : Random.Range(0, slots.Count());
                    battle.CurrentRound.EnqueueHero(slots[choosenIdx]);                                            
                    slots.RemoveAt(choosenIdx);
                }
            }
            if (battle.CurrentRound.QueuedHeroes.Count == 0)
            { 
                battle.SetRoundState(RoundState.NA);
                OnRoundEvent?.Invoke(battle.CurrentRound);
                
                battle.SetState(BattleState.Completed);
                battle.SetWinnerTeamId(-1);
                OnBattleEvent?.Invoke(battle);
            }
            else if (battle.CurrentRound.QueuedHeroes
                        .Where(x => x.TeamId == libraryManager.PlayerTeam.Id).Count() > 0 &&
                     battle.CurrentRound.QueuedHeroes
                        .Where(x => x.TeamId == libraryManager.EnemyTeam.Id).Count() > 0)
            {
                battle.SetRoundState(RoundState.RoundPrepared);
                OnRoundEvent?.Invoke(battle.CurrentRound);
            }
            else
            {
                battle.SetRoundState(RoundState.NA);
                OnRoundEvent?.Invoke(battle.CurrentRound);

                var winner = battle.CurrentRound.QueuedHeroes.First();
                battle.SetWinnerTeamId(winner.TeamId);
                battle.SetState(BattleState.Completed);
                OnBattleEvent?.Invoke(battle);
            }
            
            if (battle.Auto && CanBeginRound)
                StartCoroutine(AutoRound());
        }

        public void BeginBattle()
        {
            battle.SetState(BattleState.BattleStarted);
            OnBattleEvent?.Invoke(CurrentBattle);

            if (CanBeginRound)
                BeginRound();
        }

        public void BeginRound()
        {
            battle.SetState(BattleState.BattleInProgress);
            OnBattleEvent?.Invoke(CurrentBattle);

            battle.SetRoundState(RoundState.RoundInProgress);
            OnRoundEvent?.Invoke(battle.CurrentRound);

            PrepareNextTurn();
        }

        private void PrepareNextTurn()
        {
            var turnInfo = BattleTurnInfo.Create(battle.CurrentTurn.Turn + 1, Hero.Default, Hero.Default);
            battle.SetTurnInfo(turnInfo);
            battle.SetTurnState(TurnState.PrepareTurn);
            OnTurnEvent?.Invoke(battle.CurrentTurn);

            var turnState = PrepareTurn();
            switch (turnState)
            {
                case TurnState.NA:
                    Debug.Log("Round queue empty, next queue");
                    PrepareRound();
                    break;
                case TurnState.PrepareTurn:
                    break;
                case TurnState.TurnPrepared:
                    OnTurnEvent?.Invoke(battle.CurrentTurn);
                    break;
                case TurnState.TurnInProgress:
                    break;
                case TurnState.TurnCompleted:
                    break;
                case TurnState.NoTargets:
                    Debug.Log("Battle won!");
                    PrepareRound();
                    break;
                case TurnState.TurnProcessed:
                    break;
                default:
                    break;
            }

            if (battle.Auto && CanMakeTurn)
                StartCoroutine(AutoTurn());
        }

        internal TurnState PrepareTurn()
        {
            var allBattleHeroes = libraryManager.Library.BattleHeroes;
            var activeHeroes = allBattleHeroes.Where(x => x.HealthCurrent > 0);
            var orderedHeroes = activeHeroes.OrderByDescending(x => x.Speed);

            if (battle.CurrentRound.QueuedHeroes.Count == 0)
            {
                // round is over, prepare new one
                Debug.Log($"Round is over, prepare new one");
                return TurnState.NA;
            }
            var attaker = battle.CurrentRound.QueuedHeroes[0];
            var attackTeam = attaker.TeamId;
            var targets = activeHeroes.Where(x => x.TeamId != attackTeam).ToArray();
            var frontTargets = targets.Where(x => x.Line == BattleLine.Front).ToArray();
            var backTargets = targets.Where(x => x.Line == BattleLine.Back).ToArray();
            // TODO: consider range (not yet imported/parsed)
            var target = frontTargets.Length > 0 ?
                frontTargets[Random.Range(0, frontTargets.Length)] :
                backTargets.Length > 0 ?
                backTargets[Random.Range(0, backTargets.Length)] :
                Hero.Default;

            var turnInfo = BattleTurnInfo.Create(battle.CurrentTurn.Turn, attaker, target);
            battle.SetTurnInfo(turnInfo);

            if (target.HeroType == HeroType.NA)
            {
                // battle won, complete
                Debug.Log($"Battle won, complete");
                battle.SetTurnState(TurnState.NoTargets);
            }
            else
            {
                battle.SetTurnState(TurnState.TurnPrepared);
            }

            return battle.CurrentTurn.State;
        }

        internal void CompleteTurn()
        {

            var turnInfo = battle.CurrentTurn;
            
            // attack:
            var rawDamage = turnInfo.Attacker.RandomDamage;
            var criticalDamage = turnInfo.Attacker.RandomCriticalHit;
            var accurate = turnInfo.Attacker.RandomAccuracy;

            // defence:
            var dodged = turnInfo.Target.RandomDodge;
            var shield = turnInfo.Target.DefenceRate;

            var damage = rawDamage;
            if (!accurate || dodged)
            {
                damage = 0;
            }
            else
            {
                damage *= (criticalDamage ? 2 : 1);
                damage -= (int)Mathf.Ceil(damage * shield / 100f);
                damage = Mathf.Max(0, damage);
            }
            
            var target = turnInfo.Target.UpdateHealthCurrent(damage, out int display, out int current);

            UpdateBattleHero(target); // Sync health
            
            if (current <= 0)
                battle.CurrentRound.DequeueHero(target);

            turnInfo = BattleTurnInfo.Create(CurrentTurn, battle.CurrentTurn.Attacker, target, damage);
            turnInfo.Critical = criticalDamage;
            turnInfo.Dodged = dodged;
            turnInfo.Lethal = current <= 0;

            battle.SetTurnInfo(turnInfo);
            battle.SetTurnState(TurnState.TurnInProgress);
            
            OnTurnEvent?.Invoke(battle.CurrentTurn);

            battle.CurrentRound.QueuedHeroes.RemoveAt(0);
            
            battle.SetTurnState(TurnState.TurnCompleted);
            OnTurnEvent?.Invoke(battle.CurrentTurn);
        }

        internal void SetTurnProcessed(BattleTurnInfo stage)
        {
            battle.SetTurnInfo(stage);
            battle.SetTurnState(TurnState.TurnProcessed);
            
            if (!battle.Auto)
                OnTurnEvent?.Invoke(battle.CurrentTurn);

            OnRoundEvent?.Invoke(battle.CurrentRound);

            PrepareNextTurn();
        }


        private void UpdateBattleHero(Hero target)
        {
            libraryManager.Library.UpdateHero(target);
        }

        public void LoadData()
        {
        }
        private IEnumerator AutoRound()
        {
            yield return null;
            BeginRound();
        }
        private IEnumerator AutoTurn()
        {
            yield return null;
            CompleteTurn();
        }

        private void GiveAssetsToTeams()
        {
            var hp = Asset.EmptyAsset(AssetType.Defence, "HP", "hp");
            var shield = Asset.EmptyAsset(AssetType.Defence, "Shield", "shield");
            var bomb = Asset.EmptyAsset(AssetType.Attack, "Bomb", "bomb");
            var shuriken = Asset.EmptyAsset(AssetType.Attack, "Shuriken", "shuriken");
            var power = Asset.EmptyAsset(AssetType.Attack, "Power", "power");

            hp.GiveAsset(battle.PlayerTeam, 5);
            shield.GiveAsset(battle.PlayerTeam, 5);
            bomb.GiveAsset(battle.PlayerTeam, 5);
            shuriken.GiveAsset(battle.PlayerTeam, 5);
            power.GiveAsset(battle.PlayerTeam, 5);

            hp.GiveAsset(battle.EnemyTeam, 5);
            shield.GiveAsset(battle.EnemyTeam, 5);
            bomb.GiveAsset(battle.EnemyTeam, 5);
            shuriken.GiveAsset(battle.EnemyTeam, 5);
            power.GiveAsset(battle.EnemyTeam, 5);

        }

        internal void Autoplay()
        {
            battle.Auto = true;
            BeginRound();
        }
    }
}