using Assets.Scripts.UI.Data;
using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
    using HeroDict = Dictionary<int, Hero>;
    using Random = UnityEngine.Random;

    public class BattleManagementService : MonoBehaviour
    {
        private readonly Team playerTeam = Team.EmptyTeam(0, "Player");
        private readonly Team enemyTeam = Team.EmptyTeam(1, "Enemy");

        public event UnityAction<Hero> OnHeroUpdated;

        public Team PlayerTeam => playerTeam;
        public Team EnemyTeam => enemyTeam;

        public IEnumerable<KeyValuePair<int, Hero>> AllBattleHeroes =>
            PlayerTeam.FrontLine.Concat(PlayerTeam.BackLine)
            .Concat(EnemyTeam.FrontLine).Concat(EnemyTeam.BackLine);

        private bool resetBattle;
        private readonly List<Hero> queuedHeroes = new();

        /// <summary>
        ///     Flag to let the battle screen know if it should reset the battle/queue
        /// </summary>
        public bool ResetBattle { 
            get => resetBattle;
            internal set {
                resetBattle = value;
                if (resetBattle)
                    CurrentTurn = -1;
            } 
        }

        public void BeginBattle()
        {
            CurrentTurn = 0;
            ResetTeamHeroes();
        }

        private void ResetTeamHeroes()
        {
            ResetHealthCurrent(playerTeam.FrontLine);
            ResetHealthCurrent(playerTeam.BackLine);
            ResetHealthCurrent(enemyTeam.FrontLine);
            ResetHealthCurrent(enemyTeam.BackLine);

            Debug.Log("ResetTeamHeroes");
        }

        private static void ResetHealthCurrent(HeroDict dict)
        {
            foreach (var key in dict.Keys.ToArray())
            {
                var hero = dict[key];
                hero.HealthCurrent = hero.Health;
                dict[key] = hero;
            }
        }

        public int CurrentTurn { get; private set; }
        public int QueueLength { get; internal set; }
        public bool CanPrepareRound => queuedHeroes.Count == 0 && AllBattleHeroes.Count() > 0;
        public bool CanBeginBattle => queuedHeroes.Count > 0 && CurrentTurn < 0;
        public bool CanMakeTurn => queuedHeroes.Count > 0;

        public void PrepareTeam(HeroDict dict, int id)
        {
            var team = playerTeam.Id == id ? playerTeam : enemyTeam;
            
            for (int i = 0; i < 4; i++)
                team.BackLine[i] = Hero.Default;

            for (int i = 0; i < dict.Count; i++)
            {
                var hero = dict[i];
                hero.Line = BattleLine.Front;
                hero.HealthCurrent = hero.Health;
                team.FrontLine[i] = hero;
            }
        }
        internal Hero[] PrepareRound()
        {

            queuedHeroes.Clear();

            var allBattleHeroes = AllBattleHeroes.Select(x => x.Value);
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
                    queuedHeroes.Add(slots[choosenIdx]);
                    slots.RemoveAt(choosenIdx);
                }
            }

            ResetBattle = false;

            return queuedHeroes.ToArray();
        }
        internal Hero[] CompleteTurn()
        {
            var allBattleHeroes = AllBattleHeroes.Select(x => x.Value);
            var activeHeroes = allBattleHeroes.Where(x => x.HealthCurrent > 0);
            var orderedHeroes = activeHeroes.OrderByDescending(x => x.Speed);

            if (queuedHeroes.Count == 0)
            {
                // round is over, prepare new one
                Debug.Log($"Round is over, prepare new one");
                return queuedHeroes.ToArray();
            }
            var attaker = queuedHeroes[0];
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
                return queuedHeroes.ToArray();
            }

            var damage = Mathf.Min(target.HealthCurrent, Random.Range(attaker.DamageMin, attaker.DamageMax + 1));
            target.HealthCurrent -= damage;

            UpdateBattleHero(target); // Sync health

            Debug.Log($"{attaker.Name} attaked {target.Name}, damage: {damage}");

            OnHeroUpdated?.Invoke(target);

            queuedHeroes.RemoveAt(0);            
            
            CurrentTurn++;

            return queuedHeroes.ToArray();
        }

        private void UpdateBattleHero(Hero target)
        {
            var team = target.TeamId == playerTeam.Id ? playerTeam : enemyTeam;
            team.UpdateHero(target);
        }

        public void LoadData()
        {
            var hp = Asset.EmptyAsset(AssetType.Defence, "HP", "hp");
            var shield = Asset.EmptyAsset(AssetType.Defence, "Shield", "shield");
            var bomb = Asset.EmptyAsset(AssetType.Attack, "Bomb", "bomb");
            var shuriken = Asset.EmptyAsset(AssetType.Attack, "Shuriken", "shuriken");
            var power = Asset.EmptyAsset(AssetType.Attack, "Power", "power");

            hp.GiveAsset(playerTeam, 5);
            shield.GiveAsset(playerTeam, 5);
            bomb.GiveAsset(playerTeam, 5);
            shuriken.GiveAsset(playerTeam, 5);
            power.GiveAsset(playerTeam, 5);

            hp.GiveAsset(enemyTeam, 5);
            shield.GiveAsset(enemyTeam, 5);
            bomb.GiveAsset(enemyTeam, 5);
            shuriken.GiveAsset(enemyTeam, 5);
            power.GiveAsset(enemyTeam, 5);

        }

    }
}