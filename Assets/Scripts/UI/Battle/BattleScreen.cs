using Assets.Scripts.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Inventory;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Battle
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public partial class BattleScreen : MenuScreen
    {
        [Inject] private readonly BattleManagementService battleManager;
        [Inject] private readonly HeroLibraryService libraryManager;
        [Inject] private readonly AudioPlaybackService audioService;
        [Inject] private readonly MenuNavigationService navService;

        [SerializeField] private RectTransform playerPartyFront;
        [SerializeField] private RectTransform playerPartyBack;

        [SerializeField] private RectTransform enemyPartyFront;
        [SerializeField] private RectTransform enemyPartyBack;

        [SerializeField] private Transform playerBattleGround;        
        [SerializeField] private Transform enemyBattleGround;
        [SerializeField] private Transform attackerBattleGround;

        [SerializeField] private RaidMemberPool raidMemberPool;
        [SerializeField] private RaidMemberOverlayPool raidMemberOverlayPool;

        private readonly BattleLineSlot[] playerFrontSlots = new BattleLineSlot[4];
        private readonly BattleLineSlot[] playerBackSlots = new BattleLineSlot[4];
        private readonly BattleLineSlot[] enemyFrontSlots = new BattleLineSlot[4];
        private readonly BattleLineSlot[] enemyBackSlots = new BattleLineSlot[4];

        private SlotDelegateProvider slotDelegate = default;
        private HeroDelegateProvider heroDelegate = default;

        private readonly HeroTransfer heroTransfer = new();

        private bool initialized;
        
        private BattleQueue battleQueue;
        private UIActionButton[] actionButtons;

        private int playerTeamId;
        private int enemyTeamId;

        delegate void TransferRollback();
        TransferRollback Rollback { get; set; } // initialised on transaction start

        protected override void OnBeforeAwake()
        {
            battleQueue = GetComponent<BattleQueue>();
        }

        protected override void OnBeforeEnable()
        {
            if (initialized)
            {               
                battleManager.OnBattleEvent += BattleManager_OnBattleEvent;
                battleManager.OnRoundEvent += BattleManager_OnRoundEvent;
                battleManager.OnTurnEvent += BattleManager_OnTurnEvent;

                UpdateView();
            }
        }

        protected override void OnBeforeDisable()
        {
            battleManager.OnBattleEvent -= BattleManager_OnBattleEvent;
            battleManager.OnRoundEvent -= BattleManager_OnRoundEvent;
            battleManager.OnTurnEvent -= BattleManager_OnTurnEvent;
        }

        protected override void OnBeforeUpdate()
        { 
        }
        
        protected override void OnBeforeStart()
        {
            actionButtons = GetComponentsInChildren<UIActionButton>();
            foreach (var button in actionButtons)
                button.OnActionButtonClick += Button_OnActionButtonClick;

            battleManager.OnBattleEvent += BattleManager_OnBattleEvent;
            battleManager.OnRoundEvent += BattleManager_OnRoundEvent;
            battleManager.OnTurnEvent += BattleManager_OnTurnEvent;

            playerTeamId = libraryManager.PlayerTeam.Id;
            enemyTeamId = libraryManager.EnemyTeam.Id;

            InitBattleUnitSlotDelegates(); // drop between slots (both assets and heroes)                        

            battleManager.HeroCardFactory = raidMemberPool.CreateCard;
            battleManager.HeroOverlayFactory = raidMemberOverlayPool.CreateCard;

            raidMemberPool.CardBinder = BindHeroCard;

            InitHeroSlots(playerPartyFront, playerFrontSlots, playerTeamId, BattleLine.Front);
            InitHeroSlots(playerPartyBack, playerBackSlots, playerTeamId, BattleLine.Back);

            InitHeroSlots(enemyPartyFront, enemyFrontSlots, enemyTeamId, BattleLine.Front);
            InitHeroSlots(enemyPartyBack, enemyBackSlots, enemyTeamId, BattleLine.Back);
           
            initialized = true;

            battleManager.CreateCards((card, overlay) =>
            {
                var rm = (RaidMember)card;
                var ov = (Overlay)overlay;
                rm.HeroAnimation.SetOverlay(ov);
            });

            UpdateView();
        }

        private void UpdateView()
        {
            if (battleManager.CurrentRound.State == RoundState.RoundPrepared)
                battleQueue.LayoutHeroes(
                    battleManager.QueuedHeroes);

            UpdateActionButtons();
            ResetBattlefieldPositions();
        }

        private void ResetBattlefieldPositions()
        {
            var allSlots = playerFrontSlots.Concat(playerBackSlots).Concat(enemyFrontSlots).Concat(enemyBackSlots);
            foreach (BattleLineSlot slot in allSlots)
                if (slot.RaidMember != null)
                    slot.RaidMember.HeroAnimation.transform.localPosition = Vector3.zero;
        }

        private void SlotDelegate_HeroMoved()
        {
            SyncHeroCardSelectionWithHero();
            if (battleManager.CanReorderTurns)
                battleManager.ResetBattle();
        }

        private void InitHeroSlots(Transform containerTransform, BattleLineSlot[] slots, int teamId, BattleLine line)
        {
            foreach (var slot in containerTransform.GetComponentsInChildren<BattleLineSlot>())
            {
                slot.DelegateProvider = slotDelegate;
                slot.Position = new HeroPosition(teamId, line, slot.SlotIndex);
                slots[slot.SlotIndex] = slot;
            }
            battleManager.BindEcsHeroSlots(slots);
        }
    }


}