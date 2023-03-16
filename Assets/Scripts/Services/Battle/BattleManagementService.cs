using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using System;
using UnityEngine.Events;
using Leopotam.EcsLite;
using Assets.Scripts.UI.Battle;
using Assets.Scripts.ECS;

namespace Assets.Scripts.Services
{

    public partial class BattleManagementService : BaseEcsService
    {
        private HeroLibraryService libraryManager;
        private PlayerPreferencesService prefs;

        public event UnityAction<BattleInfo> OnBattleEvent;
        public event UnityAction<BattleRoundInfo, BattleInfo> OnRoundEvent;
        public event UnityAction<BattleTurnInfo, BattleRoundInfo, BattleInfo> OnTurnEvent;
        public event UnityAction<bool, Asset[]> OnBattleComplete;

        public BattleMode PlayMode { get; set; } = BattleMode.NA;

        /// <summary>
        /// Must have HeroConfigRefComp attached
        /// </summary>
        public EcsPackedEntityWithWorld[] PlayerTeamPackedEntities { get; set; }

        /// <summary>
        /// Must have HeroConfigRefComp attached
        /// </summary>
        public EcsPackedEntityWithWorld[] EnemyTeamPackedEntities { get; set; }

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
                BattleState.TeamsPrepared => true,
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
                BattleMode.NA => CanStartBattle,
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

        internal void RetreatBattle()
        {
            PlayMode = BattleMode.NA;
            RetreatEcsBattle();
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
            {
                if (CanStartBattle)
                {
                    CurrentBattle.SetState(BattleState.BattleStarted);
                    OnBattleEvent?.Invoke(CurrentBattle);
                }
                PlayMode = BattleMode.Fastforward;
            }
        }

        internal void MakeTurn()
        {
            MakeEcsTurn();
        }

        internal void NotiifyBattleComplete(bool won, Asset[] potAssets)
        {
            OnBattleComplete?.Invoke(won, potAssets);
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
            MenuNavigationService menuNavigationService,
            PlayerPreferencesService playerPreferencesService,
            HeroLibraryService libManagementService)
        {
            this.libraryManager = libManagementService;
            this.prefs = playerPreferencesService;

            menuNavigationService.OnBeforeNavigateToScreen += MenuNavigationService_OnBeforeNavigateToScreen;
        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens previous, Screens current)
        {
            if (current == Screens.Battle)
                StartEcsContext();

            if (previous == Screens.Battle)
                StopEcsContext();
        }

        internal EcsPackedEntityWithWorld? HeroAtPosition(Tuple<int, BattleLine, int> position) =>
            GetEcsHeroAtPosition(position);

        internal void MoveHero(EcsPackedEntityWithWorld hero, Tuple<int, BattleLine, int> pos) =>
            MoveEcsHeroToPosition(hero, pos);

        internal void RequestBattle(
            EcsPackedEntityWithWorld[] playerTeam,
            EcsPackedEntityWithWorld[] enemyTeam)
        {
            PlayerTeamPackedEntities = playerTeam;
            EnemyTeamPackedEntities = enemyTeam;
        }

    }
}