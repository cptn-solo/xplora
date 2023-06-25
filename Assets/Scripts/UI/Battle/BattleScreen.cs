using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Inventory;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen : MenuScreen
    {
        private BattleManagementService battleManager;
        private HeroLibraryService libraryManager;
        private AudioPlaybackService audioService;

        private BattleLog battleLog;

        [SerializeField] private RectTransform playerPartyFront;
        [SerializeField] private RectTransform playerPartyBack;

        [SerializeField] private RectTransform enemyPartyFront;
        [SerializeField] private RectTransform enemyPartyBack;

        private readonly BattleLineSlot[] playerFrontSlots = new BattleLineSlot[4];
        private readonly BattleLineSlot[] playerBackSlots = new BattleLineSlot[4];
        private readonly BattleLineSlot[] enemyFrontSlots = new BattleLineSlot[4];
        private readonly BattleLineSlot[] enemyBackSlots = new BattleLineSlot[4];

        private SlotDelegateProvider slotDelegate = default;

        private readonly HeroTransfer heroTransfer = new();

        private bool initialized;

        private UIActionButton[] actionButtons;

        private int playerTeamId;
        private int enemyTeamId;

        delegate void TransferRollback();
        TransferRollback Rollback { get; set; } // initialised on transaction start

        [Inject]
        public void Construct(
            BattleManagementService battleManager,
            HeroLibraryService libraryManager,
            AudioPlaybackService audioService)
        {
            this.battleManager = battleManager;
            this.libraryManager = libraryManager;
            this.audioService = audioService;

            Initialize();
        }

        private void Initialize()
        {
            battleLog = GetComponentInChildren<BattleLog>();
            actionButtons = GetComponentsInChildren<UIActionButton>();

            foreach (var button in actionButtons)
                button.OnActionButtonClick += Button_OnActionButtonClick;

            playerTeamId = libraryManager.PlayerTeam.Id;
            enemyTeamId = libraryManager.EnemyTeam.Id;

            InitBattleUnitSlotDelegates(); // drop between slots (both assets and heroes)                        

            var slots = new Dictionary<HeroPosition, IHeroPosition>();

            InitHeroSlots(slots, playerPartyFront, playerFrontSlots, playerTeamId, BattleLine.Front);
            InitHeroSlots(slots, playerPartyBack, playerBackSlots, playerTeamId, BattleLine.Back);
            InitHeroSlots(slots, enemyPartyFront, enemyFrontSlots, enemyTeamId, BattleLine.Front);
            InitHeroSlots(slots, enemyPartyBack, enemyBackSlots, enemyTeamId, BattleLine.Back);

            battleManager.BindEcsHeroSlots(slots);

            battleManager.OnBattleEvent += BattleManager_OnBattleEvent;
            battleManager.OnRoundEvent += BattleManager_OnRoundEvent;
            battleManager.OnTurnEvent += BattleManager_OnTurnEvent;

            initialized = true;
        }

        protected override void OnBeforeDestroy()
        {
            battleManager.OnBattleEvent -= BattleManager_OnBattleEvent;
            battleManager.OnRoundEvent -= BattleManager_OnRoundEvent;
            battleManager.OnTurnEvent -= BattleManager_OnTurnEvent;
        }

        protected override void OnBeforeStart() {
            if (initialized)
                UpdateView();
        }

        private void UpdateView()
        {
            UpdateActionButtons();
            ResetBattlefieldPositions();
        }

        private void ResetBattlefieldPositions()
        {
            var allSlots = playerFrontSlots.Concat(playerBackSlots).Concat(enemyFrontSlots).Concat(enemyBackSlots);
            foreach (BattleLineSlot slot in allSlots)
                if (slot.BattleUnit != null)
                    slot.BattleUnit.HeroAnimation.transform.localPosition = Vector3.zero;
        }

        private void SlotDelegate_HeroMoved()
        {
            if (battleManager.CanReorderTurns)
                battleManager.ResetBattle();
        }

        private void InitHeroSlots(Dictionary<HeroPosition, IHeroPosition> dict,
            Transform containerTransform, BattleLineSlot[] slots, int teamId, BattleLine line)
        {
            foreach (var slot in containerTransform.GetComponentsInChildren<BattleLineSlot>())
            {
                slot.DelegateProvider = slotDelegate;
                slot.Position = new HeroPosition(teamId, line, slot.SlotIndex);
                slots[slot.SlotIndex] = slot;
                dict.Add(slot.Position, slot);
            }
        }
    }


}