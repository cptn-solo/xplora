using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using Leopotam.EcsLite;

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

        public BattleMode PlayMode { get; set; } = BattleMode.NA;

        public ref BattleInfo CurrentBattle => ref GetEcsCurrentBattle();
        public ref BattleRoundInfo CurrentRound => ref GetEcsCurrentRound();
        public ref BattleTurnInfo CurrentTurn => ref GetEcsCurrentTurn();

        public int CurrentTurnNumber => CurrentTurn.Turn;

        public RoundSlotInfo[] QueuedHeroes => GetEcsRoundSlots();

        public bool CanReorderTurns =>
            CurrentBattle.State switch
            {
                BattleState.Created => true,
                BattleState.TeamsPrepared => true,
                _ => false
            };

        public bool CanStartBattle =>
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
            PlayMode switch
            {
                BattleMode.StepMode => true,
                _ => false
            };

        public bool CanAutoPlayBattle =>
            PlayMode switch
            {
                BattleMode.Autoplay => true,
                BattleMode.StepMode => true,
                _ => false
            };

        public bool CanStepPlayBattle =>
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
            DestroyEcsRounds();
        }
        public delegate void BattleCompleteDelegate(bool won);

        internal void OnBattleComplete(bool won)
        {
            if (raidService.State == RaidState.InBattle)
                raidService.ProcessAftermath(won);
            else
                nav.NavigateToScreen(Screens.HeroesLibrary);
        }

        internal void RetreatBattle()
        {
            PlayMode = BattleMode.NA;
        }

        internal void BeginBattle()
        {
            CurrentBattle.SetState(BattleState.BattleStarted);
            OnBattleEvent?.Invoke(CurrentBattle);

            PlayMode = BattleMode.Autoplay;
        }

        internal void AutoPlay()
        {
            if (CanMakeTurn)
                PlayMode = BattleMode.Autoplay;
        }
        
        internal void StepPlayBattle()
        {
            if (CanStepPlayBattle)
                PlayMode = BattleMode.StepMode;
        }

        internal void FastForwardPlay()
        {
            if (CanAutoPlayBattle)
                PlayMode = BattleMode.Fastforward;
        }

        internal void MakeTurn()
        {
            MakeEcsTurn();
        }

        public void NotifyBattleEventListeners(BattleInfo? info = null)
        {
            OnBattleEvent?.Invoke(info??CurrentBattle);
        }

        public void NotifyRoundEventListeners(BattleRoundInfo? info = null)
        {
            OnRoundEvent?.Invoke(info??CurrentRound, CurrentBattle); // to update queued members in UI/log
        }

        public void NotifyTurnEventListeners(BattleTurnInfo? info = null)
        {
            OnTurnEvent?.Invoke(info??CurrentTurn, CurrentRound, CurrentBattle); // to update queued members in UI/log
        }

        internal void SetTurnProcessed(BattleTurnInfo stage)
        {
            SetEcsTurnProcessed();
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
                StartEcsContext();

            if (previous == Screens.Battle)
            {
                UnlinkCardRefs<Hero>();
                UnlinkCardRefs<BarsAndEffectsInfo>();
                StopEcsContext();
            }
        }

        internal EcsPackedEntityWithWorld? HeroAtPosition(Tuple<int, BattleLine, int> position) =>
            GetEcsHeroAtPosition(position);

        internal void MoveHero(EcsPackedEntityWithWorld hero, Tuple<int, BattleLine, int> pos) =>
            MoveEcsHeroToPosition(hero, pos);

        internal EntityViewFactory<Hero> HeroCardFactory { get; set; }
        internal EntityViewFactory<BarsAndEffectsInfo> HeroOverlayFactory { get; set; }

    }
}