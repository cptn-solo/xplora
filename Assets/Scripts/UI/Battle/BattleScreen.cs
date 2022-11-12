using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

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
            if (arg0.FromHero.HeroType != HeroType.NA)
                heroCards[arg0.FromHero].Hero = arg0.FromHero;

            if (arg0.ToHero.HeroType != HeroType.NA)
                heroCards[arg0.ToHero].Hero = arg0.ToHero;
        }

        [SerializeField] private RectTransform playerPartyFront;
        [SerializeField] private RectTransform playerPartyBack;

        [SerializeField] private RectTransform enemyPartyFront;
        [SerializeField] private RectTransform enemyPartyBack;
        
        [SerializeField] private RectTransform teamInventory;
        [SerializeField] private RectTransform heroInventory;

        [SerializeField] private GameObject heroPrefab;

        [SerializeField] private TextMeshProUGUI heroInventoryTitle;

        private readonly UIItemSlot[] playerFrontSlots = new UIItemSlot[4];
        private readonly UIItemSlot[] playerBackSlots = new UIItemSlot[4];
        
        private readonly UIItemSlot[] teamInventorySlots = new UIItemSlot[15];
        private readonly UIItemSlot[] heroInventorySlots = new UIItemSlot[10];
        private readonly UIItemSlot[] heroAttackSlots = new UIItemSlot[2];
        private readonly UIItemSlot[] heroDefenceSlots = new UIItemSlot[2];

        private Team team = default;

        protected override void OnBeforeAwake() =>
            InitInputActions();
        protected override void OnBeforeEnable()
        {
            EnableInputActions();
            this.teamManager.OnAssetTransactionCompleted += TeamManager_OnAssetTransactionCompleted;
            this.teamManager.OnAssetTransactionAborted += TeamManager_OnAssetTransactionAborted;
        }

        protected override void OnBeforeDisable()
        {
            DisableInputActions();
            this.teamManager.OnAssetTransactionCompleted -= TeamManager_OnAssetTransactionCompleted;
            this.teamManager.OnAssetTransactionAborted -= TeamManager_OnAssetTransactionAborted;

        }

        protected override void OnBeforeUpdate() =>
            ProcessInputActions();

        protected override void OnBeforeStart()
        {

            InitPlayerHeroSlots();
            InitInventorySlots();

            team = teamManager.Team;

            foreach (var hero in team.FrontLine.Where(x => x.Value.HeroType != HeroType.NA))
                ItemForHero(hero.Value);
            
            foreach (var hero in team.BackLine.Where(x => x.Value.HeroType != HeroType.NA))
                ItemForHero(hero.Value);

            ShowTeamBatleUnits(team);
            ShowTeamInventory(team);

            selectedHero = team.FrontLine[0];
            heroCards[selectedHero].Selected = true;

            ShowHeroInventory(selectedHero);
        }

        private void InitPlayerHeroSlots()
        {
            foreach (var slot in playerPartyFront.GetComponentsInChildren<BattleLineSlot>())
            {
                slot.TransactionStart = (Transform cargo, UIItemSlot slot) =>
                    teamManager.BeginHeroTransfer(team.FrontLine, cargo.GetComponent<RaidMember>().Hero, slot.SlotIndex);
                slot.TransactionEnd = (Transform cargo, UIItemSlot slot, bool success) =>
                    teamManager.CommitHeroTransfer(team.FrontLine, slot.SlotIndex, cargo.GetComponent<RaidMember>().Hero);
                slot.TransactionAbort = () => teamManager.AbortHeroTransfer();
                slot.Validator = (Transform cargo) =>
                    cargo.GetComponent<RaidMember>() != null && slot.IsEmpty;
                playerFrontSlots[slot.SlotIndex] = slot;
            }

            foreach (var slot in playerPartyBack.GetComponentsInChildren<BattleLineSlot>())
            {
                slot.TransactionStart = (Transform cargo, UIItemSlot slot) =>
                    teamManager.BeginHeroTransfer(team.BackLine, cargo.GetComponent<RaidMember>().Hero, slot.SlotIndex);
                slot.TransactionEnd = (Transform cargo, UIItemSlot slot, bool success) =>
                    teamManager.CommitHeroTransfer(team.BackLine, slot.SlotIndex, cargo.GetComponent<RaidMember>().Hero);
                slot.TransactionAbort = () => teamManager.AbortHeroTransfer();
                slot.Validator = (Transform cargo) =>
                    cargo.GetComponent<RaidMember>() != null && slot.IsEmpty;
                playerBackSlots[slot.SlotIndex] = slot;
            }

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
                slot.TransactionStart = (Transform cargo, UIItemSlot slot) =>
                    teamManager.BeginAssetTransfer(team.Inventory, slot.SlotIndex,
                    PooledAsset(cargo));
                slot.TransactionEnd = (Transform cargo, UIItemSlot slot, bool success) =>
                    teamManager.CommitAssetTransfer(team.Inventory, slot.SlotIndex);
                slot.TransactionAbort = () => teamManager.AbortAssetTransfer();
                slot.Validator = (Transform cargo) =>
                    cargo.GetComponent<InventoryItem>() != null && slot.IsEmpty;
                teamInventorySlots[slot.SlotIndex] = slot;
            }

            foreach (var slot in heroInventory.GetComponentsInChildren<HeroInventorySlot>())
            {
                slot.TransactionStart = (Transform cargo, UIItemSlot slot) =>
                    teamManager.BeginAssetTransfer(selectedHero.Inventory, slot.SlotIndex,
                    PooledAsset(cargo), selectedHero);
                slot.TransactionEnd = (Transform cargo, UIItemSlot slot, bool success) =>
                    teamManager.CommitAssetTransfer(selectedHero.Inventory, slot.SlotIndex, selectedHero);
                slot.TransactionAbort = () => teamManager.AbortAssetTransfer();
                slot.Validator = (Transform cargo) =>
                    cargo.GetComponent<InventoryItem>() != null && slot.IsEmpty;
                heroInventorySlots[slot.SlotIndex] = slot;
            }

            foreach (var slot in heroInventory.GetComponentsInChildren<HeroAttackSlot>())
            {
                slot.TransactionStart = (Transform cargo, UIItemSlot slot) =>
                    teamManager.BeginAssetTransfer(selectedHero.Attack, slot.SlotIndex,
                    PooledAsset(cargo), selectedHero);
                slot.TransactionEnd = (Transform cargo, UIItemSlot slot, bool success) =>
                    teamManager.CommitAssetTransfer(selectedHero.Attack, slot.SlotIndex, selectedHero);
                slot.TransactionAbort = () => teamManager.AbortAssetTransfer();
                slot.Validator = (Transform cargo) =>
                    cargo.GetComponent<InventoryItem>() != null && slot.IsEmpty;
                heroAttackSlots[slot.SlotIndex] = slot;
            }

            foreach (var slot in heroInventory.GetComponentsInChildren<HeroDefenceSlot>())
            {
                slot.TransactionStart = (Transform cargo, UIItemSlot slot) =>
                    teamManager.BeginAssetTransfer(selectedHero.Defence, slot.SlotIndex,
                    PooledAsset(cargo), selectedHero);
                slot.TransactionEnd = (Transform cargo, UIItemSlot slot, bool success) =>
                    teamManager.CommitAssetTransfer(selectedHero.Defence, slot.SlotIndex, selectedHero);
                slot.TransactionAbort = () => teamManager.AbortAssetTransfer();
                slot.Validator = (Transform cargo) =>
                    cargo.GetComponent<InventoryItem>() != null && slot.IsEmpty;
                heroDefenceSlots[slot.SlotIndex] = slot;
            }
        }
    }


}