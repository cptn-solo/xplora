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
        public event UnityAction<BattleRoundInfo, BattleInfo> OnRoundEvent;
        public event UnityAction<BattleTurnInfo, BattleRoundInfo, BattleInfo> OnTurnEvent;

        private BattleInfo battle;
        private readonly int minRoundsQueue = 3;

        public BattleInfo CurrentBattle => battle;

        public int CurrentTurn => battle.CurrentTurn.Turn;
        public bool CanReorderTurns => battle.State switch
        {
            BattleState.Created => true,
            BattleState.TeamsPrepared => true,
            _ => false
        }; //  queuedHeroes.Count == 0 && AllBattleHeroes.Count() > 0;

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

            if (libraryManager.PrepareTeamsForBattle(out var playerHeroes, out var enemyHeroes))
            {
                battle.SetHeroes(playerHeroes, enemyHeroes);
                battle.SetState(BattleState.TeamsPrepared);
                OnBattleEvent?.Invoke(battle);

                StartCoroutine(PrepareNextRounds());
            }

        }

        private IEnumerator PrepareNextRounds()
        {
            if (battle.CurrentRound.State == RoundState.RoundCompleted)
                battle.RoundsQueue.RemoveAt(0);

            while (battle.RoundsQueue.Count < minRoundsQueue)
            {
                var lastId = battle.RoundsQueue.Count > 0 ? battle.RoundsQueue.Last().Round : -1;
                var draftRound = BattleRoundInfo.Create(lastId + 1);
                var preparedRound = PrepareRound(draftRound);
                battle.EnqueueRound(preparedRound);
                yield return null;
            }
            
            OnRoundEvent?.Invoke(battle.CurrentRound, battle);

            PrepareNextTurn();
        }

        /// <summary>
        ///     Enqueue heroes 
        /// </summary>
        /// <param name="info">Draft round</param>
        /// <returns>Round with heroes</returns>
        internal BattleRoundInfo PrepareRound(BattleRoundInfo info)
        {
            info.SetState(RoundState.PrepareRound);
            info.ResetQueue();

            var activeHeroes = battle.PlayerHeroes.Concat(battle.EnemyHeroes);
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
                    info.EnqueueHero(slots[choosenIdx]);                                            
                    slots.RemoveAt(choosenIdx);
                }
            }

            info.SetState(RoundState.RoundPrepared);

            return info;
        }

        private void PrepareNextTurn()
        {

            var turnInfo = BattleTurnInfo.Create(battle.CurrentTurn.Turn + 1, Hero.Default, Hero.Default);
            battle.SetTurnInfo(turnInfo);
            battle.SetTurnState(TurnState.PrepareTurn);
            OnTurnEvent?.Invoke(battle.CurrentTurn, battle.CurrentRound, battle);

            var turnState = PrepareTurn();
            if (turnState == TurnState.TurnPrepared)
                OnTurnEvent?.Invoke(battle.CurrentTurn, battle.CurrentRound, battle);

            if (battle.Auto && CanMakeTurn)
                StartCoroutine(AutoTurn());
        }

        internal TurnState PrepareTurn()
        {
            var attaker = battle.CurrentRound.QueuedHeroes[0];
            var attackTeam = attaker.TeamId;
            var targets = attackTeam == battle.PlayerTeam.Id ?
                battle.EnemyHeroes.ToArray() : battle.PlayerHeroes.ToArray();
            
            Hero target = default;
            if (attaker.Ranged)
            {
                target = targets[Random.Range(0, targets.Length)];
            }
            else
            {
                var frontTargets = targets.Where(x => x.Line == BattleLine.Front).ToArray();
                var backTargets = targets.Where(x => x.Line == BattleLine.Back).ToArray();
                // TODO: consider range (not yet imported/parsed)
                target = frontTargets.Length > 0 ?
                    frontTargets[Random.Range(0, frontTargets.Length)] :
                    backTargets.Length > 0 ?
                    backTargets[Random.Range(0, backTargets.Length)] :
                    Hero.Default;
            }

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

        /// <summary>
        ///     Can safely be converted into a coroutine
        /// </summary>
        internal void CompleteTurn()
        {
            if (battle.State == BattleState.TeamsPrepared)
            {
                battle.SetState(BattleState.BattleStarted);
                OnBattleEvent?.Invoke(battle);
            }

            battle.SetState(BattleState.BattleInProgress);
            OnBattleEvent?.Invoke(battle);

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

                if (DamageEffectInfo.TryCast(
                        turnInfo.Attacker,
                        turnInfo.Target,
                        battle.CurrentRound.Round,
                        out var damageEffect)
                    )
                {
                    for (int i = 0; i <= damageEffect.TurnsActive; i++)
                    {
                        var roundInfo = battle.RoundsQueue[i];
                        var targetIdx = roundInfo.QueuedHeroes
                            .FindIndex(x => x.Id == damageEffect.Hero.Id);

                        var targetHero = roundInfo.QueuedHeroes[targetIdx];
                        // consider conditional effect addition (stackable/nonstackable effects)
                        targetHero.Effects.Add(damageEffect);
                        roundInfo.QueuedHeroes[targetIdx] = targetHero;
                        battle.RoundsQueue[i] = roundInfo;
                    }
                }
            }

            var target = turnInfo.Target.UpdateHealthCurrent(damage, out int display, out int current);

            UpdateBattleHero(target); // Sync health                        

            turnInfo = BattleTurnInfo.Create(CurrentTurn, battle.CurrentTurn.Attacker, target, damage);
            turnInfo.Critical = criticalDamage;
            turnInfo.Dodged = dodged;
            turnInfo.Lethal = current <= 0;

            battle.SetTurnInfo(turnInfo);
            
            battle.SetTurnState(TurnState.TurnInProgress);            
            OnTurnEvent?.Invoke(battle.CurrentTurn, battle.CurrentRound, battle);
            
            battle.SetTurnState(TurnState.TurnCompleted);
            OnTurnEvent?.Invoke(battle.CurrentTurn, battle.CurrentRound, battle);
        }

        internal void SetTurnProcessed(BattleTurnInfo stage)
        {
            battle.SetTurnInfo(stage);
            battle.SetTurnState(TurnState.TurnProcessed);

            battle.CurrentRound.QueuedHeroes.RemoveAt(0);

            var winnerTeamId = CheckForWinner();
            if (winnerTeamId < 0)
            {
                if (battle.CurrentRound.QueuedHeroes.Count > 0)
                {
                    battle.SetRoundState(RoundState.RoundInProgress);
                    OnRoundEvent?.Invoke(battle.CurrentRound, battle);

                    PrepareNextTurn();
                }
                else
                {
                    battle.SetRoundState(RoundState.RoundCompleted);
                    OnRoundEvent?.Invoke(battle.CurrentRound, battle);

                    StartCoroutine(PrepareNextRounds());
                }                    
            }
            else
            {
                battle.SetRoundState(RoundState.RoundCompleted);
                OnRoundEvent?.Invoke(battle.CurrentRound, battle);

                battle.SetState(BattleState.Completed);
                battle.SetWinnerTeamId(winnerTeamId);
                OnBattleEvent?.Invoke(battle);
            }
        }
        private int CheckForWinner()
        {
            if (battle.PlayerHeroes.Count > 0 &&
                battle.EnemyHeroes.Count > 0)
                return -1;
            else if (battle.PlayerHeroes.Count > 0)
                return battle.PlayerTeam.Id;
            else
                return battle.EnemyTeam.Id;
        }

        private void UpdateBattleHero(Hero target)
        {
            battle.UpdateHero(target);

            libraryManager.Library.UpdateHero(target);
        }

        public void LoadData()
        {
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
            StartCoroutine(AutoTurn());
        }
    }
}