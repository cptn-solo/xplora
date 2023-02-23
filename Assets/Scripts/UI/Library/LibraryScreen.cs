using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System;
using System.Linq;
using UnityEngine;
using Zenject;
using Leopotam.EcsLite;

namespace Assets.Scripts.UI.Library
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public partial class LibraryScreen : MenuScreen
    {
        [Inject] private readonly HeroLibraryService libManager;
        [Inject] private readonly BattleManagementService battleManager;
        [Inject] private readonly MenuNavigationService nav;
        
        [SerializeField] private Transform libraryContainer;
        [SerializeField] private Transform playerTeamContainer;
        [SerializeField] private Transform enemyTeamContainer;
        
        [SerializeField] private HeroDetailsHover heroDetails;

        [SerializeField] private HeroCardPool cardPool;

        [SerializeField] private UIMenuButton raidButton;

        private EcsPackedEntityWithWorld? selectedHero = null;

        private bool googleHeroesAvailable = true;

        private readonly LibrarySlot[] librarySlots = new LibrarySlot[24];
        private readonly PlayerTeamSlot[] playerSlots = new PlayerTeamSlot[4];
        private readonly EnemyTeamSlot[] enemySlots = new EnemyTeamSlot[4];

        private SlotDelegateProvider slotDelegate = default;
        private bool initialized;
        private readonly HeroTransfer heroTransfer = new();

        delegate void TransferRollback();
        TransferRollback Rollback { get; set; }// initialised on transaction start

        protected override void OnBeforeStart()
        {
            InitSlotDelegates();

            var actionButtons = GetComponentsInChildren<UIActionButton>(true);

#if !PLATFORM_STANDALONE_WIN && !UNITY_EDITOR
            googleHeroesAvailable = false;
#endif

            foreach (var button in actionButtons)
            {
                button.gameObject.SetActive(false);

                if (button.Action == Actions.ReloadMetadata &&
                    !googleHeroesAvailable) continue;

                button.gameObject.SetActive(true);
                button.OnActionButtonClick += OnActionButtonPressed;
            }

            heroDetails.DataLoader = libManager.GetDataForPackedEntity<Hero>;

            libManager.HeroCardFactory = cardPool.CreateHeroCard;
            libManager.OnDataAvailable += LibManager_OnDataAvailable;

            cardPool.CardBinder = BindHeroCard;
            
            InitSlots(libraryContainer, librarySlots, -1);
            InitSlots(playerTeamContainer, playerSlots, 0);
            InitSlots(enemyTeamContainer, enemySlots, 1);


            initialized = true;
            
            if (libManager.DataAvailable)
                LibManager_OnDataAvailable();
        }

        protected override void OnBeforeEnable()
        {
            if (initialized)
            {
                libManager.OnDataAvailable += LibManager_OnDataAvailable;

                if (libManager.DataAvailable)
                    LibManager_OnDataAvailable();
            }
        }

        protected override void OnBeforeDisable()
        {
            libManager.OnDataAvailable -= LibManager_OnDataAvailable;
        }

        private void LibManager_OnDataAvailable()
        {
            libManager.CreateCards();

            SyncWorldAndButton();
        }

        private void ShowTeamCards(int teamId)
        {
            var heroes = teamId == 0 ? libManager.PlayerHeroes : libManager.EnemyHeroes;
            TeamMemberSlot[] slots = teamId == 0 ? playerSlots : enemySlots;
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                var hero = i < heroes.Count() ? heroes[i] : Hero.Default;
                slot.Hero = hero;
            }
        }

        private void InitSlots<T>(Transform container, T[] outSlots, int teamId) where T: LibrarySlot
        {
            var slots = container.GetComponentsInChildren<T>();
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];

                var line = (slot is TeamMemberSlot) ? BattleLine.Front : BattleLine.NA;

                slot.Position = new HeroPosition(teamId, line, i); 
                slot.SlotIndex = i;
                slot.DelegateProvider = slotDelegate;
                outSlots[i] = slot;
            }
            libManager.BindEcsLibraryScreenHeroSlots(outSlots);
        }

        private void SyncHeroCardSelectionWithHero()
        {
            foreach (var slots in new[] { librarySlots, playerSlots, enemySlots })
                foreach (var card in slots
                    .Where(x => x.HeroCard != null)
                    .Select(x => x.HeroCard)
                    .ToArray())
                    card.Selected = card.PackedEntity.Equals(selectedHero);
        }
        private void SyncWorldAndButton()
        {
            var playerHeroes = libManager.PlayerHeroes;
            
            raidButton.gameObject.SetActive(playerHeroes.Length > 0);

        }

        private void BindHeroCard(HeroCard heroCard)
        {
            var actionButton = heroCard.GetComponent<UIActionButton>();
            actionButton.OnActionButtonClick += OnActionButtonPressed;
        }


        private void OnActionButtonPressed(Actions action, Transform actionTransform)
        {
            switch (action)
            {
                case Actions.SelectHero:
                    {
                        var heroCard = actionTransform.GetComponent<HeroCard>();
                        Debug.Log($"Hero from line #{heroCard.Hero} selected");

                        selectedHero = heroCard.PackedEntity;
                        SyncHeroCardSelectionWithHero();
                        //ShowHeroInventory(selectedHero);

                        heroDetails.PackedEntity = selectedHero;
                        heroDetails.Update();
                    }
                    break;
                case Actions.SaveTeamForBattle:
                    {                        
                        nav.NavigateToScreen(Screens.Battle);
                    }
                    break;
                case Actions.ReloadMetadata:
                    {
                        if (googleHeroesAvailable)
                            libManager.LoadGoogleData();
                    }
                    break;
                default:
                    break;
            }

        }

    }
}