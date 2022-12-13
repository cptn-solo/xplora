using Assets.Scripts.Services.App;
using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Battle
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public partial class BattleScreen : MenuScreen
    {
        [Inject] private readonly BattleManagementService battleManager;
        [Inject] private readonly HeroLibraryManagementService libraryManager;
        [Inject] private readonly AudioPlaybackService audioService;
        [Inject] private readonly MenuNavigationService navService;

        [SerializeField] private RectTransform playerPartyFront;
        [SerializeField] private RectTransform playerPartyBack;

        [SerializeField] private RectTransform enemyPartyFront;
        [SerializeField] private RectTransform enemyPartyBack;

        [SerializeField] private RectTransform playerTeamInventory;
        [SerializeField] private RectTransform enemyTeamInventory;
        [SerializeField] private RectTransform heroInventory;

        [SerializeField] private TextMeshProUGUI heroInventoryTitle;

        [SerializeField] private Transform playerBattleGround;        
        [SerializeField] private Transform enemyBattleGround;

        private readonly BattleLineSlot[] playerFrontSlots = new BattleLineSlot[4];
        private readonly BattleLineSlot[] playerBackSlots = new BattleLineSlot[4];
        private readonly BattleLineSlot[] enemyFrontSlots = new BattleLineSlot[4];
        private readonly BattleLineSlot[] enemyBackSlots = new BattleLineSlot[4];

        private readonly TeamInventorySlot[] playerTeamInventorySlots =
            new TeamInventorySlot[15];
        private readonly TeamInventorySlot[] enemyTeamInventorySlots =
            new TeamInventorySlot[15];
        private readonly HeroInventorySlot[] heroInventorySlots =
            new HeroInventorySlot[10];

        private readonly HeroAttackSlot[] heroAttackSlots = new HeroAttackSlot[2];
        private readonly HeroDefenceSlot[] heroDefenceSlots = new HeroDefenceSlot[2];

        private SlotDelegateProvider slotDelegate = default;
        private HeroDelegateProvider heroDelegate = default;


        private readonly AssetTransfer assetTransfer = new();
        private readonly HeroTransfer heroTransfer = new();

        private bool initialized;
        private bool inventoryToggle = false;
        
        private BattleInventory battleInventory;
        private BattleQueue battleQueue;
        private UIActionButton[] actionButtons;

        private UIActionToggleButton toggleInventoryButton;

        private int playerTeamId;
        private int enemyTeamId;

        delegate void TransferRollback();
        TransferRollback Rollback { get; set; } // initialised on transaction start

        protected override void OnBeforeAwake()
        {
            battleInventory = GetComponent<BattleInventory>();
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

            audioService.Stop();
        }

        protected override void OnBeforeUpdate()
        { 
        }
        
        protected override void OnBeforeStart()
        {
            actionButtons = GetComponentsInChildren<UIActionButton>();
            foreach (var button in actionButtons)
            {
                button.OnActionButtonClick += Button_OnActionButtonClick;

                if (button.Action == Actions.ToggleInventoryPanel)
                    toggleInventoryButton = (UIActionToggleButton)button;
            }

            battleManager.OnBattleEvent += BattleManager_OnBattleEvent;
            battleManager.OnRoundEvent += BattleManager_OnRoundEvent;
            battleManager.OnTurnEvent += BattleManager_OnTurnEvent;

            battleInventory.Toggle(inventoryToggle);
            toggleInventoryButton.Toggle(inventoryToggle);
            battleQueue.Toggle(!inventoryToggle);

            playerTeamId = libraryManager.PlayerTeam.Id;
            enemyTeamId = libraryManager.EnemyTeam.Id;

            InitInventorySlotDelegates(); // drop between slots (both assets and heroes)
            
            InitTeamInventorySlots(playerTeamInventory, playerTeamInventorySlots,
                playerTeamId);            
            InitTeamInventorySlots(enemyTeamInventory, enemyTeamInventorySlots,
                enemyTeamId);
            
            InitInventorySlots(heroInventory, heroInventorySlots);
            InitInventorySlots(heroInventory, heroAttackSlots);
            InitInventorySlots(heroInventory, heroDefenceSlots);

            InitRaidMemberDelegates(); // drop on hero cards

            InitHeroSlots(playerPartyFront, playerFrontSlots, playerTeamId, BattleLine.Front);
            InitHeroSlots(playerPartyBack, playerBackSlots, playerTeamId, BattleLine.Back);

            InitHeroSlots(enemyPartyFront, enemyFrontSlots, enemyTeamId, BattleLine.Front);
            InitHeroSlots(enemyPartyBack, enemyBackSlots, enemyTeamId, BattleLine.Back);
            
            initialized = true;

            UpdateView();
        }

        private void UpdateView()
        {
            ShowTeamBatleUnits(playerTeamId);
            ShowTeamBatleUnits(enemyTeamId);
            ShowTeamInventory(libraryManager.PlayerTeam);
            ShowTeamInventory(libraryManager.EnemyTeam);

            if (battleManager.CurrentBattle.CurrentRound.State == RoundState.RoundPrepared)
                battleQueue.LayoutHeroes(
                    battleManager.CurrentBattle.QueuedHeroes);


            UpdateActionButtons();
            ResetBattlefieldPositions();
        }
        private void ResetBattlefieldPositions()
        {
            var allSlots = playerFrontSlots.Concat(playerBackSlots).Concat(enemyFrontSlots).Concat(enemyBackSlots);
            foreach (BattleLineSlot slot in allSlots)
                slot.RaidMember.HeroAnimation.transform.localPosition = Vector3.zero;
        }


        private void SlotDelegate_HeroUpdated(Hero hero) =>
            RaidMemberForHero(hero).Hero = hero;

        private void SlotDelegate_HeroMoved(Hero hero)
        {
            SyncHeroCardSelectionWithHero();
        }
        
        private void ToggleInventory()
        {
            inventoryToggle = !inventoryToggle;
            toggleInventoryButton.Toggle(inventoryToggle);

            battleInventory.Toggle(inventoryToggle);
            battleQueue.Toggle(!inventoryToggle);
        }

        private void InitHeroSlots(Transform containerTransform, BattleLineSlot[] slots, int teamId, BattleLine line)
        {
            foreach (var slot in containerTransform.GetComponentsInChildren<BattleLineSlot>())
            {
                slot.DelegateProvider = slotDelegate;
                slot.Position = new HeroPosition(teamId, line, slot.SlotIndex);
                slots[slot.SlotIndex] = slot;
            }
        }
        
        private void InitTeamInventorySlots(Transform containerTransform, 
            TeamInventorySlot[] slots, int teamId)
        {
            foreach (var slot in containerTransform
                .GetComponentsInChildren<TeamInventorySlot>())
            {
                slot.DelegateProvider = slotDelegate;
                slot.SetTeamId(teamId);
                slots[slot.SlotIndex] = slot;
            }
        }

        private void InitInventorySlots<T>(Transform containerTransform, T[] slots)
            where T : AssetInventorySlot
        {
            foreach (var slot in containerTransform.GetComponentsInChildren<T>())
            {
                slot.DelegateProvider = slotDelegate;
                slots[slot.SlotIndex] = slot;
            }
        }
    }


}