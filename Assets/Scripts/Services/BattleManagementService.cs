using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Assets.Scripts
{
    using static UnityEngine.Rendering.DebugUI;
    using HeroDict = Dictionary<int, Hero>;
    using Random = UnityEngine.Random;

    public class BattleManagementService : MonoBehaviour
    {
        [Inject] private readonly HeroLibraryManagementService libraryManager;

        public event UnityAction<BattleInfo> OnBattleEvent;
        public event UnityAction<BattleRoundInfo> OnRoundEvent;
        public event UnityAction<BattleTurnInfo> OnTurnEvent;

        private BattleInfo battle;

        public Team PlayerTeam => battle.PlayerTeam;
        public Team EnemyTeam => battle.EnemyTeam;

        public Hero CurrentAttacker => battle.CurrentRound.CurrentAttacker;

        /// <summary>
        ///     Flag to let the battle screen know if it should reset the battle/queue
        /// </summary>
        public void ResetBattle()
        {
            battle = BattleInfo.Create();
            GiveAssetsToTeams();

            OnBattleEvent?.Invoke(battle);
        }

        public void BeginBattle()
        {
            libraryManager.ResetHealthCurrent();
        }

        public int CurrentTurn => battle.CurrentTurn.Turn;
        public int QueueLength { get; internal set; }
        public bool CanPrepareRound => battle.State == BattleState.PrepareRound; //  queuedHeroes.Count == 0 && AllBattleHeroes.Count() > 0;
        public bool CanBeginBattle => battle.State == BattleState.RoundPrepared; //queuedHeroes.Count > 0 && CurrentTurn < 0
        public bool CanMakeTurn => battle.State == BattleState.TurnCompleted; //queuedHeroes.Count > 0;

        /// <summary>
        ///     Initial team layout as it comes from the library (all to front line)
        /// </summary>
        public void InitBattleTeams()
        {
            battle.SetState(BattleState.TeamsPrepared);
            OnBattleEvent?.Invoke(battle);
        }

        public void MoveHero(Hero hero)
        {

        }    

        internal Hero[] PrepareRound()
        {

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

            return battle.CurrentRound.QueuedHeroes.ToArray();
        }
        internal Hero[] CompleteTurn()
        {
            var allBattleHeroes = libraryManager.Library.BattleHeroes;
            var activeHeroes = allBattleHeroes.Where(x => x.HealthCurrent > 0);
            var orderedHeroes = activeHeroes.OrderByDescending(x => x.Speed);

            if (battle.CurrentRound.QueuedHeroes.Count == 0)
            {
                // round is over, prepare new one
                Debug.Log($"Round is over, prepare new one");
                return battle.CurrentRound.QueuedHeroes.ToArray();
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
                return battle.CurrentRound.QueuedHeroes.ToArray();
            }

            var damage = Mathf.Min(target.HealthCurrent, Random.Range(attaker.DamageMin, attaker.DamageMax + 1));
            target.HealthCurrent -= damage;

            UpdateBattleHero(target); // Sync health

            var turn = BattleTurnInfo.Create(CurrentTurn, attaker, target);
            OnTurnEvent?.Invoke(turn);

            Debug.Log($"{attaker.Name} attaked {target.Name}, damage: {damage}");

            battle.CurrentRound.QueuedHeroes.RemoveAt(0);            
            
            battle.CurrentTurn.Increment();

            return battle.CurrentRound.QueuedHeroes.ToArray();
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