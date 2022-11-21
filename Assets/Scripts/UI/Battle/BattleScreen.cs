using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;
using Asset = Assets.Scripts.UI.Data.Asset;

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

        [SerializeField] private RectTransform inventoryPanel;
        [SerializeField] private RectTransform battleQueuePanel;
        [SerializeField] private TextMeshProUGUI heroInventoryTitle;

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
        private bool inventoryToggle = true;

        delegate void TransferRollback();
        TransferRollback Rollback { get; set; } // initialised on transaction start

        protected override void OnBeforeAwake() =>
            InitInputActions();
        protected override void OnBeforeEnable()
        {
            if (initialized)
            {
                ShowTeamBatleUnits(battleManager.PlayerTeam);
                ShowTeamInventory(battleManager.PlayerTeam);
                
                ShowTeamBatleUnits(battleManager.EnemyTeam);
                ShowTeamInventory(battleManager.EnemyTeam);
            }

            EnableInputActions();
        }

        protected override void OnBeforeDisable() =>
            DisableInputActions();

        protected override void OnBeforeUpdate() =>
            ProcessInputActions();

        private bool CheckSlot(UIItemSlot slot)
        {
            if (!slot.IsEmpty)
                return false;

            if (slot is AssetInventorySlot)
            {
                if (slot is HeroAttackSlot)
                    return assetTransfer.TransferAsset.AssetType == AssetType.Attack;
                else if (slot is HeroDefenceSlot)
                    return assetTransfer.TransferAsset.AssetType == AssetType.Defence;
                else
                    return assetTransfer.TransferAsset.AssetType != AssetType.NA;
            }
            else if (slot is BattleLineSlot bls)            
                return heroTransfer.TransferHero.HeroType != HeroType.NA &&
                    heroTransfer.TransferHero.TeamId == bls.TeamId;
            else
                return false;
        }
        
        protected override void OnBeforeStart()
        {
            var actionButtons = GetComponentsInChildren<UIActionButton>();
            foreach (var button in actionButtons)
                button.OnActionButtonClick += Button_OnActionButtonClick;

            inventoryPanel.gameObject.SetActive(inventoryToggle);
            battleQueuePanel.gameObject.SetActive(!inventoryToggle);

            InitInventorySlotDelegates(); // drop between slots (both assets and heroes)
            
            InitTeamInventorySlots(playerTeamInventory, playerTeamInventorySlots, 
                battleManager.PlayerTeam.Id);            
            InitTeamInventorySlots(enemyTeamInventory, enemyTeamInventorySlots,
                battleManager.EnemyTeam.Id);
            
            InitInventorySlots(heroInventory, heroInventorySlots);
            InitInventorySlots(heroInventory, heroAttackSlots);
            InitInventorySlots(heroInventory, heroDefenceSlots);

            InitRaidMemberDelegates(); // drop on hero cards

            InitHeroSlots(playerPartyFront, playerFrontSlots, battleManager.PlayerTeam.Id);
            InitHeroSlots(playerPartyBack, playerBackSlots, battleManager.PlayerTeam.Id);

            InitHeroSlots(enemyPartyFront, enemyFrontSlots, battleManager.EnemyTeam.Id);
            InitHeroSlots(enemyPartyBack, enemyBackSlots, battleManager.EnemyTeam.Id);

            ShowTeamBatleUnits(battleManager.PlayerTeam);
            ShowTeamInventory(battleManager.PlayerTeam);

            ShowTeamBatleUnits(battleManager.EnemyTeam);
            ShowTeamInventory(battleManager.EnemyTeam);

            selectedHero = battleManager.PlayerTeam.FrontLine[0];
            SyncHeroCardSelectionWithHero();
            ShowHeroInventory(selectedHero);

            initialized = true;
        }

        private void Button_OnActionButtonClick(Actions arg1, Transform arg2)
        {
            switch (arg1)
            {                
                case Actions.ToggleInventoryPanel:
                    {
                        var toggleButton = arg2.GetComponent<UIActionToggleButton>();
                        inventoryToggle = !inventoryToggle;
                        toggleButton.Toggle(inventoryToggle);

                        var pos = inventoryPanel.position;
                        pos.y = inventoryToggle ? 0f : -350f;

                        inventoryPanel.position = pos;
                        inventoryPanel.gameObject.SetActive(inventoryToggle);
                        battleQueuePanel.gameObject.SetActive(!inventoryToggle);

                    }
                    break;
                default:
                    break;
            }
        }

        private void InitHeroSlots(Transform containerTransform, BattleLineSlot[] slots, int teamId)
        {
            foreach (var slot in containerTransform.GetComponentsInChildren<BattleLineSlot>())
            {
                slot.DelegateProvider = slotDelegate;
                slot.SetTeamId(teamId);
                slots[slot.SlotIndex] = slot;
            }
        }
        private Transform PooledItem(UIItemSlot slot)
        {
            var sample = slot.transform.childCount == 0 ? null : slot.transform.GetChild(0);
            if (slot is AssetInventorySlot)
            {
                return PooledInventoryItem(sample);
            }
            else if (slot is BattleLineSlot)
            {
                return PooledHeroItem(sample);
            }
            else
                return null;
        }
        private Transform PooledHeroItem(Transform placeholder)
        {
            if (placeholder == null) // create and bind a new card
            {
                var placeholderCard = assetPool.GetRaidMember(
                    Hero.Default, canvas.transform.localScale)
                    .transform.GetComponent<RaidMember>();
                BindHeroCard(placeholderCard); //placeholders are just filled with data on cargo drop
                return placeholderCard.transform;
            }
            else // grab a card from the pool for display purposes
            {
                var placeholderCard = placeholder
                    .transform.GetComponent<RaidMember>();
                var hero = placeholderCard.Hero;
                var card = assetPool.GetRaidMember(hero, canvas.transform.localScale);
                return card.transform;

            }
        }

        private Transform PooledInventoryItem(Transform placeholder)
        {
            if (placeholder == null) // new asset card for placeholder
            {
                var placeholderCard = assetPool.GetAssetCard(
                    default, canvas.transform.localScale)
                    .transform.GetComponent<InventoryItem>();
                return placeholderCard.transform;
            }
            else // grab a card from the pool for display purposes
            {
                var placeholderCard = placeholder
                    .transform.GetComponent<InventoryItem>();
                var asset = placeholderCard.Asset;
                var card = assetPool.GetAssetCard(asset, canvas.transform.localScale);
                return card.transform;
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