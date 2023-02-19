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

        private bool battleRouterCoroutineRunning;

        private BattleMode playMode = BattleMode.NA;

        public BattleMode PlayMode => playMode;

        private bool turnCoroutineRunning;
        
        private bool retreatBattleRunning;
        private readonly int minRoundsQueue = 4;

        public ref BattleInfo CurrentBattle => ref GetEcsCurrentBattle();
        public ref BattleRoundInfo CurrentRound => ref GetEcsCurrentRound();
        public ref BattleTurnInfo CurrentTurn => ref GetEcsCurrentTurn();

        public int CurrentTurnNumber => CurrentTurn.Turn;

        private void SetTurnState(TurnState state) =>
            CurrentTurn.State = state;

        private void SetTurnInfo(BattleTurnInfo info, TurnState state) =>
            SetEcsCurrentTurnInfo(info, state);

        private void UpdateHero(Hero target)
        {
            throw new System.NotImplementedException();
        }

        private void RemoveCompletedRounds() =>
            RemoveEcsCompletedRounds();

        public bool CanReorderTurns =>
            !retreatBattleRunning &&
            CurrentBattle.State switch
            {
                BattleState.Created => true,
                BattleState.TeamsPrepared => true,
                _ => false
            };

        public bool CanStartBattle =>
            !retreatBattleRunning &&
            CurrentBattle.State switch
            {
                BattleState.TeamsPrepared => 
                CurrentRound.State switch
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
            CurrentTurn.State switch
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

            StopEcsContext();
            StartEcsContext();
            
            playMode = BattleMode.NA;

            GiveAssetsToTeams();

            CurrentBattle.SetState(BattleState.PrepareTeams);
            OnBattleEvent?.Invoke(CurrentBattle);

            if (libraryManager.PrepareTeamsForBattle(out var playerHeroes, out var enemyHeroes))
            {
                CurrentBattle.SetState(BattleState.TeamsPrepared);
                OnBattleEvent?.Invoke(CurrentBattle);

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
            CurrentBattle.SetState(BattleState.BattleStarted);
            OnBattleEvent?.Invoke(CurrentBattle);

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

            if (CurrentBattle.State == BattleState.BattleStarted ||
                CurrentBattle.State == BattleState.BattleInProgress)
            {
                CurrentBattle.SetWinnerTeamId(CurrentBattle.EnemyTeam.Id);
                CurrentBattle.SetState(BattleState.Completed);

                OnBattleEvent?.Invoke(CurrentBattle);

                yield return new WaitForSeconds(2.0f);
            }

            if (raidService.State == RaidState.InBattle)
                raidService.ProcessAftermath(
                    CurrentBattle.WinnerTeamId == libraryManager.PlayerTeam.Id);
            else
                nav.NavigateToScreen(Screens.HeroesLibrary);

            retreatBattleRunning = false;
        }


        private IEnumerator PrepareNextRoundsCoroutine()
        {
            RemoveCompletedRounds();

            while (GetEcsRoundsCount() is var count && count < minRoundsQueue)
            {
                EnqueueEcsRound();
                yield return null;
            }

            OnRoundEvent?.Invoke(CurrentRound, CurrentBattle); // to update queued members in UI/log
        }


        private void PrepareNextTurn()
        {
            var turnInfo = BattleTurnInfo.Create(CurrentTurn.Turn + 1, Hero.Default, Hero.Default);
            SetTurnInfo(turnInfo, TurnState.PrepareTurn);
            OnTurnEvent?.Invoke(CurrentTurn, CurrentRound, CurrentBattle);

            PrepareTurn();
        }

        internal IEnumerator CompleteTurn()
        {
            turnCoroutineRunning = true;

            var turnInfo = CurrentTurn;
            
            //capture value just in case
            var skipTurn = turnInfo.State == TurnState.TurnSkipped;

            ApplyQueuedEffects(turnInfo, out var attacker, out var effectsInfo);

            yield return null;

            if (effectsInfo != null)
            {
                UpdateBattleHero(attacker); // Sync health
                OnTurnEvent?.Invoke((BattleTurnInfo)effectsInfo, CurrentRound, CurrentBattle);
            }

            if (attacker.HealthCurrent <=0 || skipTurn)
            {
                turnInfo = turnInfo.UpdateAttacker(attacker);
                SetTurnInfo(turnInfo, TurnState.TurnCompleted);
                
                yield return null;
            }
            else
            {
                ProcessAttack(turnInfo, attacker, out var target, out var resultInfo);

                UpdateBattleHero(target); // Sync health

                SetTurnInfo(resultInfo, TurnState.TurnInProgress);
                OnTurnEvent?.Invoke(CurrentTurn, CurrentRound, CurrentBattle);
                
                yield return null;
                
                SetTurnState(TurnState.TurnCompleted);
            }
            
            yield return null;

            turnCoroutineRunning = false;
        }

        internal void SetTurnProcessed(BattleTurnInfo stage)
        {
            FinalizeTurn();

            SetTurnInfo(stage, TurnState.TurnProcessed);
        }
        private int CheckForWinner()
        {
            if (PlayerHeroes.Length > 0 &&
                EnemyHeroes.Length > 0)
                return -1;
            else if (PlayerHeroes.Length > 0)
                return CurrentBattle.PlayerTeam.Id;
            else
                return CurrentBattle.EnemyTeam.Id;
        }

        private void UpdateBattleHero(Hero target)
        {
            UpdateHero(target);
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

            hp.GiveAsset(CurrentBattle.PlayerTeam, 5);
            shield.GiveAsset(CurrentBattle.PlayerTeam, 5);
            bomb.GiveAsset(CurrentBattle.PlayerTeam, 5);
            shuriken.GiveAsset(CurrentBattle.PlayerTeam, 5);
            power.GiveAsset(CurrentBattle.PlayerTeam, 5);

            hp.GiveAsset(CurrentBattle.EnemyTeam, 5);
            shield.GiveAsset(CurrentBattle.EnemyTeam, 5);
            bomb.GiveAsset(CurrentBattle.EnemyTeam, 5);
            shuriken.GiveAsset(CurrentBattle.EnemyTeam, 5);
            power.GiveAsset(CurrentBattle.EnemyTeam, 5);

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