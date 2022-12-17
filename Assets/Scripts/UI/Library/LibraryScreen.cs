using Assets.Scripts.Services;
using Assets.Scripts.Services.App;
using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Library
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public partial class LibraryScreen : MenuScreen
    {
        [Inject] private readonly HeroLibraryManagementService libManager;
        [Inject] private readonly BattleManagementService battleManager;
        [Inject] private readonly MenuNavigationService nav;
        [Inject] private readonly AudioPlaybackService audioService;


        [SerializeField] private Transform libraryContainer;
        [SerializeField] private Transform playerTeamContainer;
        [SerializeField] private Transform enemyTeamContainer;
        
        [SerializeField] private HeroDetailsHover heroDetails;

        [SerializeField] private HeroCardPool cardPool;

        private Hero selectedHero = Hero.Default;

        private bool googleHeroesAvailable = true;

        private readonly LibrarySlot[] librarySlots = new LibrarySlot[24];
        private readonly PlayerTeamSlot[] playerSlots = new PlayerTeamSlot[4];
        private readonly EnemyTeamSlot[] enemySlots = new EnemyTeamSlot[4];

        private SlotDelegateProvider slotDelegate = default;
        private HeroesLibrary library;
        private bool initialized;
        private readonly HeroTransfer heroTransfer = new();

        delegate void TransferRollback();
        TransferRollback Rollback { get; set; }// initialised on transaction start

        protected override void OnBeforeStart()
        {
            InitSlotDelegates();

            var actionButtons = GetComponentsInChildren<UIActionButton>();

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

            library = libManager.Library;

            InitSlots(libraryContainer, librarySlots, -1);
            InitSlots(playerTeamContainer, playerSlots, library.PlayerTeam.Id);
            InitSlots(enemyTeamContainer, enemySlots, library.EnemyTeam.Id);

            libManager.OnDataAvailable += LibManager_OnDataAvailable;

            initialized = true;

            if (libManager.DataAvailable)
                LibManager_OnDataAvailable();
        }

        protected override void OnBeforeEnable()
        {
            if (initialized && libManager.DataAvailable)
            {
                LibManager_OnDataAvailable();
            }
        }
        protected override void OnBeforeDisable()
        {
            audioService.Stop();
        }

        private void LibManager_OnDataAvailable()
        {            
            ShowHeroesLibraryCards();
            ShowPlayerCards();
            ShowEnemyCards();

            audioService.Play(SFX.LibraryTheme);
        }

        private void ShowHeroesLibraryCards()
        {
            foreach (var slot in librarySlots)
                slot.Hero = library.HeroAtPosition(slot.Position);
        }
        private void ShowPlayerCards()
        {
            foreach (var slot in playerSlots)
                slot.Hero = library.HeroAtPosition(slot.Position);
        }
        private void ShowEnemyCards()
        {
            foreach (var slot in enemySlots)
                slot.Hero = library.HeroAtPosition(slot.Position);
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
        }
        private void SyncHeroCardSelectionWithHero()
        {
            foreach (var slots in new[] { librarySlots, playerSlots, enemySlots })
                foreach (var card in slots.Select(x => x.HeroCard).ToArray())
                    card.Selected = card.Hero.Id == selectedHero.Id;
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

                        selectedHero = heroCard.Hero;
                        SyncHeroCardSelectionWithHero();
                        //ShowHeroInventory(selectedHero);

                        heroDetails.Hero = selectedHero;
                    }
                    break;
                case Actions.SaveTeamForBattle:
                    {                        
                        battleManager.ResetBattle();

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