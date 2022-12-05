using Assets.Scripts.UI.Data;
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
        public bool CanBeginBattle => battle.CurrentRound.State switch
        {
            RoundState.RoundPrepared => true,
            _ => false
        }; //queuedHeroes.Count > 0 && CurrentTurn < 0
        public bool CanMakeTurn => battle.CurrentTurn.State switch
        {
            TurnState.TurnPrepared => true,
            _ => false
        }; //queuedHeroes.Count > 0;        


        /// <summary>
        ///     Flag to let the battle screen know if it should reset the battle/queue
        /// </summary>
        public void ResetBattle()
        {
            libraryManager.ResetTeams();            
            battle = BattleInfo.Create(libraryManager.PlayerTeam, libraryManager.EnemyTeam);

            GiveAssetsToTeams();

            battle.SetState(BattleState.PrepareTeams);
            OnBattleEvent?.Invoke(battle);

            battle.SetRoundState(RoundState.PrepareRound);
            OnRoundEvent?.Invoke(battle.CurrentRound);
        }

        public void MoveHero(Hero hero)
        {

        }

        internal void PrepareRound()
        {
            battle.SetRoundState(RoundState.PrepareRound);

            battle.CurrentRound.QueuedHeroes.Clear();

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
                    battle.CurrentRound.QueuedHeroes.Add(slots[choosenIdx]);
                    slots.RemoveAt(choosenIdx);
                }
            }

            battle.SetRoundState(RoundState.RoundPrepared);

            OnRoundEvent?.Invoke(battle.CurrentRound);
        }

        public void BeginBattle()
        {
            libraryManager.ResetHealthCurrent();

            battle.SetState(BattleState.BattleInProgress);
            OnBattleEvent?.Invoke(CurrentBattle);

            battle.SetRoundState(RoundState.RoundInProgress);
            OnRoundEvent?.Invoke(battle.CurrentRound);

            battle.SetTurnState(TurnState.PrepareTurn);
            OnTurnEvent?.Invoke(battle.CurrentTurn);

            if (PrepareTurn() == TurnState.TurnPrepared)
            {
                OnTurnEvent?.Invoke(battle.CurrentTurn);
            }
            else
            {
                Debug.Log("Can't prepare turn");
            }
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

            if (target.HeroType == HeroType.NA)
            {
                // battle won, complete
                Debug.Log($"Battle won, complete");
                return TurnState.NoTargets;
            }

            var turnInfo = BattleTurnInfo.Create(battle.CurrentTurn.Turn + 1, attaker, target);
            turnInfo.SetState(TurnState.TurnPrepared);
            battle.SetTurnInfo(turnInfo);

            return turnInfo.State;

        }
        internal void CompleteTurn()
        {

            var turn = battle.CurrentTurn;
            var damage = Mathf.Min(
                turn.Target.HealthCurrent, 
                Random.Range(turn.Attacker.DamageMin, turn.Attacker.DamageMax + 1));
            var hp = turn.Target.HealthCurrent - damage;
            
            var target = turn.Target.UpdateHealthCurrent(hp);

            UpdateBattleHero(target); // Sync health

            turn = BattleTurnInfo.Create(CurrentTurn, battle.CurrentTurn.Attacker, target, damage);
            battle.SetTurnInfo(turn);
            battle.SetTurnState(TurnState.TurnInProgress);
            OnTurnEvent?.Invoke(battle.CurrentTurn);

            battle.CurrentRound.QueuedHeroes.RemoveAt(0);
            
            battle.SetTurnState(TurnState.TurnCompleted);
            OnTurnEvent?.Invoke(battle.CurrentTurn);
        }

        private void UpdateBattleHero(Hero target)
        {
            libraryManager.Library.UpdateHero(target);
        }

        public void LoadData()
        {
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

    }
}