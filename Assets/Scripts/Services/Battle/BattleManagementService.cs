using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Services
{
    public partial class BattleManagementService : MonoBehaviour
    {
        private HeroLibraryService libraryManager;
        private PlayerPreferencesService prefs;
        private MenuNavigationService nav;
        private RaidService raidService;

        public event UnityAction<BattleInfo> OnBattleEvent;
        public event UnityAction<BattleRoundInfo, BattleInfo> OnRoundEvent;
        public event UnityAction<BattleTurnInfo, BattleRoundInfo, BattleInfo> OnTurnEvent;

        private BattleInfo battle;

        private bool battleRouterCoroutineRunning;

        private BattleMode playMode = BattleMode.NA;

        public BattleMode PlayMode => playMode;

        private bool turnCoroutineRunning;
        
        private bool retreatBattleRunning;
        private readonly int minRoundsQueue = 4;

        public BattleInfo CurrentBattle => battle;

        public int CurrentTurn => battle.CurrentTurn.Turn;

        public bool CanReorderTurns =>
            !retreatBattleRunning &&
            battle.State switch
            {
                BattleState.Created => true,
                BattleState.TeamsPrepared => true,
                _ => false
            };

        public bool CanStartBattle =>
            !retreatBattleRunning &&
            battle.State switch
            {
                BattleState.TeamsPrepared => 
                battle.CurrentRound.State switch
                {
                    RoundState.RoundPrepared => true,
                    _ => false
                },
                _ => false
            };

        public bool CanMakeTurn =>
            !retreatBattleRunning &&
            !turnCoroutineRunning &&
            PlayMode switch
            {
                BattleMode.StepMode => true,
                _ => false
            };

        public bool CanAutoPlayBattle =>
            !retreatBattleRunning &&
            PlayMode switch
            {
                BattleMode.Autoplay => true,
                BattleMode.StepMode => true,
                _ => false
            };

        public bool CanStepPlayBattle =>
            !retreatBattleRunning &&
            PlayMode switch
            {
                BattleMode.Autoplay => true,
                BattleMode.Fastforward => true,
                _ => false
            };

        public bool CanCompleteTurn =>
            battle.CurrentTurn.State switch
            {
                TurnState.TurnPrepared => true,
                TurnState.TurnSkipped => true,
                _ => false
            };

        /// <summary>
        ///     Flag to let the battle screen know if it should reset the battle/queue
        /// </summary>
        public void ResetBattle()
        {
            libraryManager.ResetTeams();
            battle = BattleInfo.Create(libraryManager.PlayerTeam, libraryManager.EnemyTeam);
            
            playMode = BattleMode.NA;

            GiveAssetsToTeams();

            battle.SetState(BattleState.PrepareTeams);
            OnBattleEvent?.Invoke(battle);

            if (libraryManager.PrepareTeamsForBattle(out var playerHeroes, out var enemyHeroes))
            {
                battle.SetHeroes(playerHeroes, enemyHeroes);
                battle.SetState(BattleState.TeamsPrepared);
                OnBattleEvent?.Invoke(battle);

                StartCoroutine(PrepareNextRoundsCoroutine());
            }
        }
        internal void RetreatBattle()
        {
            if (!retreatBattleRunning)
                StartCoroutine(RetreatBattleCoroutine());
        }

        internal void BeginBattle()
        {
            battle.SetState(BattleState.BattleStarted);
            OnBattleEvent?.Invoke(battle);

            playMode = BattleMode.Autoplay;

            if (!battleRouterCoroutineRunning)
                StartCoroutine(BattleRouterCoroutine());
        }

        internal void AutoPlay()
        {
            if (CanMakeTurn)
                playMode = BattleMode.Autoplay;
        }
        
        internal void StepPlayBattle()
        {
            if (CanStepPlayBattle)
                playMode = BattleMode.StepMode;
        }

        internal void FastForwardPlay()
        {
            if (CanAutoPlayBattle)
                playMode = BattleMode.Fastforward;
        }

        internal void MakeTurn()
        {
            if (!turnCoroutineRunning)
                StartCoroutine(CompleteTurn());
        }
        private IEnumerator RetreatBattleCoroutine()
        {
            retreatBattleRunning = true;

            playMode = BattleMode.NA;

            while (turnCoroutineRunning)
                yield return null;

            if (battle.State == BattleState.BattleStarted ||
                battle.State == BattleState.BattleInProgress)
            {
                battle.SetWinnerTeamId(battle.EnemyTeam.Id);
                battle.SetState(BattleState.Completed);

                OnBattleEvent?.Invoke(battle);

                yield return new WaitForSeconds(2.0f);
            }

            if (raidService.State == RaidState.InBattle)
                raidService.ProcessAftermath(
                    battle.WinnerTeamId == libraryManager.PlayerTeam.Id);
            else
                nav.NavigateToScreen(Screens.HeroesLibrary);


            retreatBattleRunning = false;
        }


        private IEnumerator PrepareNextRoundsCoroutine()
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

            OnRoundEvent?.Invoke(battle.CurrentRound, battle); // to update queued members in UI/log
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

            PrepareTurn();
        }

        internal void PrepareTurn()
        {
            var roundSlot = battle.CurrentRound.QueuedHeroes[0];
            var attacker = libraryManager.HeroById(roundSlot.HeroId);

            if (attacker.SkipTurnActive)
            {
                var skippedInfo = BattleTurnInfo.Create(battle.CurrentTurn.Turn, attacker,
                    0, attacker.ActiveEffects.Keys.ToArray());
                battle.SetTurnInfo(skippedInfo);
                battle.SetTurnState(TurnState.TurnSkipped);
            }
            else
            {
                var attackTeam = roundSlot.TeamId;
                var targets = attackTeam == battle.PlayerTeam.Id ?
                    battle.EnemyHeroes.ToArray() : battle.PlayerHeroes.ToArray();

                Hero target = default;
                if (attacker.Ranged)
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

                var turnInfo = BattleTurnInfo.Create(battle.CurrentTurn.Turn, attacker, target,
                    0, attacker.ActiveEffects.Keys.ToArray(), null);
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
            }
        }

        internal IEnumerator CompleteTurn()
        {
            turnCoroutineRunning = true;

            var turnInfo = battle.CurrentTurn;
            
            //capture value just in case
            var skipTurn = turnInfo.State == TurnState.TurnSkipped;

            ApplyQueuedEffects(turnInfo, out var attacker, out var effectsInfo);

            yield return null;

            if (effectsInfo != null)
            {
                UpdateBattleHero(attacker); // Sync health
                OnTurnEvent?.Invoke((BattleTurnInfo)effectsInfo, battle.CurrentRound, battle);
            }

            if (attacker.HealthCurrent <=0 || skipTurn)
            {
                turnInfo = turnInfo.UpdateAttacker(attacker);
                battle.SetTurnInfo(turnInfo);
                battle.SetTurnState(TurnState.TurnCompleted);
                
                yield return null;
            }
            else
            {
                ProcessAttack(turnInfo, attacker, out var target, out var resultInfo);

                UpdateBattleHero(target); // Sync health

                battle.SetTurnInfo(resultInfo);
                battle.SetTurnState(TurnState.TurnInProgress);
                OnTurnEvent?.Invoke(battle.CurrentTurn, battle.CurrentRound, battle);
                
                yield return null;
                
                battle.SetTurnState(TurnState.TurnCompleted);
            }
            
            yield return null;

            turnCoroutineRunning = false;
        }

        internal void SetTurnProcessed(BattleTurnInfo stage)
        {
            battle.CurrentRound.FinalizeTurn();

            battle.SetTurnInfo(stage);
            battle.SetTurnState(TurnState.TurnProcessed);
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
            libraryManager.UpdateHero(target);

            if (raidService.State == RaidState.InBattle)
                raidService.ProcessBattleHeroUpdate(target);

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

        internal void Init(
            PlayerPreferencesService playerPreferencesService,
            HeroLibraryService libManagementService,
            MenuNavigationService menuNavigationService,
            RaidService raidService)
        {
            this.libraryManager = libManagementService;
            this.nav = menuNavigationService;
            this.raidService = raidService;
            this.prefs = playerPreferencesService;

            menuNavigationService.OnBeforeNavigateToScreen += MenuNavigationService_OnBeforeNavigateToScreen;

        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens previous, Screens current)
        {
            if (current == Screens.Battle)
                ResetBattle();
        }
    }
}