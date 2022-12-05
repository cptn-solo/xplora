using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System;
using System.Collections;
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

        private void UpdateActionButtons()
        {
            foreach (var button in actionButtons)
            {
                if (button.Action == Actions.PrepareQueue)
                    button.gameObject.SetActive(battleManager.CanPrepareRound);

                if (button.Action == Actions.BeginBattle)
                    button.gameObject.SetActive(battleManager.CanBeginBattle);

                if (button.Action == Actions.CompleteTurn)
                    button.gameObject.SetActive(battleManager.CanMakeTurn);

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

            UpdateActionButtons();
        }

        private void SlotDelegate_HeroUpdated(Hero hero) =>
            RaidMemberForHero(hero).Hero = hero;

        private void SlotDelegate_HeroMoved(Hero hero)
        {
            SyncHeroCardSelectionWithHero();
            battleManager.MoveHero(hero);
        }

        private void BattleManager_OnBattleEvent(BattleInfo battleInfo)
        {
            switch (battleInfo.State)
            {
                case BattleState.NA:
                    break;
                case BattleState.Created:
                    {
                        ShowTeamInventory(libraryManager.PlayerTeam);
                        ShowTeamInventory(libraryManager.EnemyTeam);
                    }
                    break;
                case BattleState.PrepareTeams:
                    break;
                case BattleState.TeamsPrepared:
                    break;
                case BattleState.PrepareRound:
                    break;
                case BattleState.RoundPrepared:
                    break;
                case BattleState.PrepareTurn:
                    break;
                case BattleState.TurnPrepared:
                    break;
                case BattleState.TurnInProgress:
                    break;
                case BattleState.TurnCompleted:
                    break;
                case BattleState.RoundCompleted:
                    break;
                case BattleState.NoTargets:
                    break;
                case BattleState.Completed:
                    break;
                default:
                    break;
            }
        }
        private void BattleManager_OnTurnEvent(BattleTurnInfo turnInfo)
        {
            StartCoroutine(SetTurnAnimation(turnInfo));
        }
        private void BattleManager_OnRoundEvent(BattleRoundInfo roundInfo)
        {
        }

        private void Button_OnActionButtonClick(Actions arg1, Transform arg2)
        {
            switch (arg1)
            {                
                case Actions.ToggleInventoryPanel:
                    {
                        ToggleInventory();
                    }
                    break;
                case Actions.BeginBattle:
                    {
                        if (inventoryToggle)
                            ToggleInventory();

                        battleManager.BeginBattle();

                        UpdateActionButtons();
                    }
                    break;

                case Actions.PrepareQueue:
                    {
                        if (inventoryToggle)
                            ToggleInventory();

                        var queuedHeroes = battleManager.PrepareRound();
                        battleQueue.LayoutHeroes(queuedHeroes);

                        UpdateActionButtons();
                    }
                    break;
                case Actions.CompleteTurn:
                    {
                        var queuedHeroes = battleManager.CompleteTurn();
                        battleQueue.CompleteTurn(queuedHeroes);

                        UpdateActionButtons();
                    }
                    break;
                default:
                    break;
            }
        }
        private IEnumerator SetTurnAnimation(BattleTurnInfo info)
        {
            var attackerAnimation =
                info.Attacker.TeamId == libraryManager.PlayerTeam.Id ?
                playerBattleGround : enemyBattleGround;
            var targetAnimation =
                info.Target.TeamId == libraryManager.PlayerTeam.Id ?
                playerBattleGround : enemyBattleGround;

            var attakerRM = RaidMemberForHero(info.Attacker);
            var targetRM = RaidMemberForHero(info.Target);

            attakerRM.HeroAnimation.Attack();

            yield return new WaitForSeconds(.5f);

            targetRM.HeroAnimation.Hit(info.Lethal);

            yield return new WaitForSeconds(1f);

            // ??
            targetRM.Hero = info.Target;

            battleQueue.UpdateHero(info.Target);
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