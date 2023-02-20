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
        
        private bool retreatBattleRunning;
        private readonly int minRoundsQueue = 4;

        public ref BattleInfo CurrentBattle => ref GetEcsCurrentBattle();
        public ref BattleRoundInfo CurrentRound => ref GetEcsCurrentRound();
        public ref BattleTurnInfo CurrentTurn => ref GetEcsCurrentTurn();

        public int CurrentTurnNumber => CurrentTurn.Turn;

        public RoundSlotInfo[] QueuedHeroes => GetEcsRoundSlots();

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

            CurrentBattle.SetState(BattleState.PrepareTeams);
            OnBattleEvent?.Invoke(CurrentBattle);

            if (libraryManager.PrepareTeamsForBattle(out var playerHeroes, out var enemyHeroes))
            {
                CurrentBattle.SetState(BattleState.TeamsPrepared);
                OnBattleEvent?.Invoke(CurrentBattle);

                StartCoroutine(BattleEcsRunloopCoroutine());
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
            MakeEcsTurn();
        }
        private IEnumerator RetreatBattleCoroutine()
        {
            retreatBattleRunning = true;

            playMode = BattleMode.NA;

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


        public void NotifyRoundEventListeners()
        {
            OnRoundEvent?.Invoke(CurrentRound, CurrentBattle); // to update queued members in UI/log
        }

        public void NotifyTurnEventListeners(BattleTurnInfo? info = null)
        {
            OnTurnEvent?.Invoke(info??CurrentTurn, CurrentRound, CurrentBattle); // to update queued members in UI/log
        }

        internal void SetTurnProcessed(BattleTurnInfo stage)
        {
            SetEcsTurnProcessed();
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

            if (previous == Screens.Battle)
            {
                StopAllCoroutines();
                StopEcsContext();
            }


        }
    }
}