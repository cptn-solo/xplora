using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;
using Asset = Assets.Scripts.UI.Data.Asset;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen : MenuScreen
    {
        private TeamManagementService teamManager = null;

        [Inject]
        public void Construct(TeamManagementService teamManager)
        {
            this.teamManager = teamManager;           
        }

        private void TeamManager_OnAssetTransactionAborted(TeamManagementService.AssetTransaction arg0)
        {
        }

        private void TeamManager_OnAssetTransactionCompleted(TeamManagementService.AssetTransaction arg0)
        {
            if (arg0.FromHero.HeroType != HeroType.NA ||
                arg0.ToHero.HeroType != HeroType.NA)
                ShowTeamBatleUnits(team);
        }
        private void TeamManager_OnHeroTransactionAborted(TeamManagementService.HeroTransaction arg0)
        {
        }

        private void TeamManager_OnHeroTransactionCompleted(TeamManagementService.HeroTransaction arg0)
        {
        }


        [SerializeField] private RectTransform playerPartyFront;
        [SerializeField] private RectTransform playerPartyBack;

        [SerializeField] private RectTransform enemyPartyFront;
        [SerializeField] private RectTransform enemyPartyBack;
        
        [SerializeField] private RectTransform teamInventory;
        [SerializeField] private RectTransform heroInventory;

        [SerializeField] private TextMeshProUGUI heroInventoryTitle;

        private readonly BattleLineSlot[] playerFrontSlots = new BattleLineSlot[4];
        private readonly BattleLineSlot[] playerBackSlots = new BattleLineSlot[4];
        
        private readonly TeamInventorySlot[] teamInventorySlots = new TeamInventorySlot[15];
        private readonly HeroInventorySlot[] heroInventorySlots = new HeroInventorySlot[10];
        private readonly HeroAttackSlot[] heroAttackSlots = new HeroAttackSlot[2];
        private readonly HeroDefenceSlot[] heroDefenceSlots = new HeroDefenceSlot[2];

        private Team team = default;
        private SlotDelegateProvider slotDelegate = default;
        private HeroDelegateProvider heroDelegate = default;
        delegate void TransferRollback();
        TransferRollback Rollback { get; set; }// initialised on transaction start

        protected override void OnBeforeAwake() =>
            InitInputActions();
        protected override void OnBeforeEnable()
        {
            EnableInputActions();
            this.teamManager.OnAssetTransactionCompleted += TeamManager_OnAssetTransactionCompleted;
            this.teamManager.OnAssetTransactionAborted += TeamManager_OnAssetTransactionAborted;
            this.teamManager.OnHeroTransactionCompleted += TeamManager_OnHeroTransactionCompleted;
            this.teamManager.OnHeroTransactionAborted += TeamManager_OnHeroTransactionAborted;
        }

        protected override void OnBeforeDisable()
        {
            DisableInputActions();
            this.teamManager.OnAssetTransactionCompleted -= TeamManager_OnAssetTransactionCompleted;
            this.teamManager.OnAssetTransactionAborted -= TeamManager_OnAssetTransactionAborted;
            this.teamManager.OnHeroTransactionCompleted -= TeamManager_OnHeroTransactionCompleted;
            this.teamManager.OnHeroTransactionAborted -= TeamManager_OnHeroTransactionAborted;
        }

        protected override void OnBeforeUpdate() =>
            ProcessInputActions();

        private bool CheckSlot(UIItemSlot slot)
        {
            if (!slot.IsEmpty)
                return false;

            if (slot is AssetInventorySlot)
                return teamManager.TransferAsset.AssetType != AssetType.NA;
            else if (slot is BattleLineSlot)            
                return teamManager.TransferHero.HeroType != HeroType.NA;
            else
                return false;
        }
        
        protected override void OnBeforeStart()
        {
            InitInventorySlotDelegates(); // drop between slots (both assets and heroes)
            InitRaidMemberDelegates(); // drop on hero cards

            InitPlayerHeroSlots();
            InitInventorySlots();

            team = teamManager.Team;

            ShowTeamBatleUnits(team);
            ShowTeamInventory(team);

            selectedHero = team.FrontLine[0];
            playerFrontSlots[0].RaidMember.Selected = true;

            ShowHeroInventory(selectedHero);
        }

        private Dictionary<int, Asset> GetAssetDictForSlot(UIItemSlot s)
        {
            return s is HeroInventorySlot ? selectedHero.Inventory :
                                    s is HeroDefenceSlot ? selectedHero.Defence :
                                    s is HeroAttackSlot ? selectedHero.Attack :
                                    team.Inventory;
        }

        private void InitPlayerHeroSlots()
        {
            foreach (var slot in playerPartyFront.GetComponentsInChildren<BattleLineSlot>())
            {
                
                slot.DelegateProvider = slotDelegate;
                playerFrontSlots[slot.SlotIndex] = slot;
            }

            foreach (var slot in playerPartyBack.GetComponentsInChildren<BattleLineSlot>())
            {
                slot.DelegateProvider = slotDelegate;
                playerBackSlots[slot.SlotIndex] = slot;
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
        private Transform PooledHeroItem(Transform sample = null)
        {
            if (sample == null)
                sample = ItemForHero(default).transform;

            var heroCard = sample.GetComponent<RaidMember>();
            var card = assetPool.GetHeroCard(heroCard.Hero);
            return card.transform;

        }

        private Transform PooledInventoryItem(Transform sample = null)
        {
            if (sample == null)
                sample = ItemForAsset(default).transform;

            var inventoryItem = sample.GetComponent<InventoryItem>();
            var card = assetPool.GetAssetCard(inventoryItem.Asset);
            return card.transform;

        }
        private Asset PooledAsset(Transform cargo)
        {
            cargo.SetParent(assetPool.transform);
            return cargo.GetComponent<InventoryItem>().Asset;
        }
        private void InitInventorySlots()
        {
            foreach (var slot in teamInventory.GetComponentsInChildren<TeamInventorySlot>())
            {
                slot.DelegateProvider = slotDelegate;
                teamInventorySlots[slot.SlotIndex] = slot;
            }

            foreach (var slot in heroInventory.GetComponentsInChildren<HeroInventorySlot>())
            {
                slot.DelegateProvider = slotDelegate;
                heroInventorySlots[slot.SlotIndex] = slot;
            }

            foreach (var slot in heroInventory.GetComponentsInChildren<HeroAttackSlot>())
            {
                slot.DelegateProvider = slotDelegate;
                heroAttackSlots[slot.SlotIndex] = slot;
            }

            foreach (var slot in heroInventory.GetComponentsInChildren<HeroDefenceSlot>())
            {
                slot.DelegateProvider = slotDelegate;
                heroDefenceSlots[slot.SlotIndex] = slot;
            }
        }
    }


}