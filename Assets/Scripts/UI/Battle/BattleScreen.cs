using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen : MenuScreen
    {
        [Inject] private readonly BattleManagementService battleManager;

        [SerializeField] private RectTransform playerPartyFront;
        [SerializeField] private RectTransform playerPartyBack;

        [SerializeField] private RectTransform enemyPartyFront;
        [SerializeField] private RectTransform enemyPartyBack;

        [SerializeField] private RectTransform playerTeamInventory;
        [SerializeField] private RectTransform enemyTeamInventory;
        [SerializeField] private RectTransform heroInventory;

        [SerializeField] private TextMeshProUGUI heroInventoryTitle;

        [SerializeField] private HeroAnimation playerHeroAnimation;        
        [SerializeField] private HeroAnimation enemyHeroAnimation;

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
                ShowTeamBatleUnits(battleManager.PlayerTeam);
                ShowTeamInventory(battleManager.PlayerTeam);

                ShowTeamBatleUnits(battleManager.EnemyTeam);
                ShowTeamInventory(battleManager.EnemyTeam);

                UpdateActionButtons();

                battleManager.OnHeroUpdated += BattleManager_OnHeroUpdated;
                battleManager.OnAttack += BattleManager_OnAttack;
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
            battleManager.OnHeroUpdated -= BattleManager_OnHeroUpdated;
            battleManager.OnAttack -= BattleManager_OnAttack;
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

            battleManager.OnHeroUpdated += BattleManager_OnHeroUpdated;
            battleManager.OnAttack += BattleManager_OnAttack;

            battleInventory.Toggle(inventoryToggle);
            toggleInventoryButton.Toggle(inventoryToggle);
            battleQueue.Toggle(!inventoryToggle);

            InitInventorySlotDelegates(); // drop between slots (both assets and heroes)
            
            InitTeamInventorySlots(playerTeamInventory, playerTeamInventorySlots, 
                battleManager.PlayerTeam.Id);            
            InitTeamInventorySlots(enemyTeamInventory, enemyTeamInventorySlots,
                battleManager.EnemyTeam.Id);
            
            InitInventorySlots(heroInventory, heroInventorySlots);
            InitInventorySlots(heroInventory, heroAttackSlots);
            InitInventorySlots(heroInventory, heroDefenceSlots);

            InitRaidMemberDelegates(); // drop on hero cards

            InitHeroSlots(playerPartyFront, playerFrontSlots, battleManager.PlayerTeam.Id, BattleLine.Front);
            InitHeroSlots(playerPartyBack, playerBackSlots, battleManager.PlayerTeam.Id, BattleLine.Back);

            InitHeroSlots(enemyPartyFront, enemyFrontSlots, battleManager.EnemyTeam.Id, BattleLine.Front);
            InitHeroSlots(enemyPartyBack, enemyBackSlots, battleManager.EnemyTeam.Id, BattleLine.Back);

            ShowTeamBatleUnits(battleManager.PlayerTeam);
            ShowTeamInventory(battleManager.PlayerTeam);

            ShowTeamBatleUnits(battleManager.EnemyTeam);
            ShowTeamInventory(battleManager.EnemyTeam);

            selectedHero = battleManager.PlayerTeam.FrontLine[0];
            SyncHeroCardSelectionWithHero();
            ShowHeroInventory(selectedHero);

            playerHeroAnimation.Initialize();
            enemyHeroAnimation.Initialize();

            initialized = true;

            UpdateActionButtons();
        }

        private void SlotDelegate_HeroUpdated(Hero hero) =>
            RaidMemberForHero(hero).Hero = hero;

        private void SlotDelegate_HeroMoved(Hero hero) =>
            SyncHeroCardSelectionWithHero();

        private void BattleManager_OnHeroUpdated(Hero hero)
        {
            var raidMember = RaidMemberForHero(hero);
            if (raidMember != default)
                raidMember.Hero = hero;

            battleQueue.UpdateHero(hero);
        }

        private void BattleManager_OnAttack(AttackInfo info)
        {
            StartCoroutine(SetAttackAnimation(info));
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

                        SetCurrentAttackerAnimation(battleManager.CurrentAttacker);

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
        private IEnumerator SetAttackAnimation(AttackInfo info)
        {
            var attackerAnimation =
                info.Attacker.TeamId == battleManager.PlayerTeam.Id ?
                playerHeroAnimation : enemyHeroAnimation;
            var targetAnimation =
                info.Target.TeamId == battleManager.PlayerTeam.Id ?
                playerHeroAnimation : enemyHeroAnimation;


            attackerAnimation.SetHero(info.Attacker);
            targetAnimation.SetHero(info.Target);

            attackerAnimation.Attack();
            yield return new WaitForSeconds(.5f);
            targetAnimation.Hit(info.Lethal);
            yield return new WaitForSeconds(.5f);
            SetCurrentAttackerAnimation(battleManager.CurrentAttacker);

        }
        private void SetCurrentAttackerAnimation(Hero attacker)
        {
            if (attacker.HeroType == HeroType.NA)
            {
                playerHeroAnimation.SetHero(Hero.Default);
                enemyHeroAnimation.SetHero(Hero.Default);
                return;

            }

            playerHeroAnimation.SetHero(
                attacker.TeamId == battleManager.PlayerTeam.Id ?
                attacker : Hero.Default);
            enemyHeroAnimation.SetHero(
                attacker.TeamId == battleManager.EnemyTeam.Id ?
                attacker : Hero.Default);
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
                slot.SetTeamId(teamId);
                slot.SetLine(line);
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