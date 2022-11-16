using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Library
{
    public partial class LibraryScreen : MenuScreen
    {
        [SerializeField] private Transform libraryContainer;
        [SerializeField] private HeroCardPool cardPool;

        private HeroLibraryManagementService libManager = null;

        [Inject]
        public void Construct(HeroLibraryManagementService libManager)
        {
            this.libManager = libManager;
        }

        private Hero selectedHero = Hero.Default;

        private readonly LibrarySlot[] librarySlots = new LibrarySlot[24];
        private SlotDelegateProvider slotDelegate = default;
        private HeroesLibrary library;
        private LibrarySlot[] libraryHeroSlots;

        delegate void TransferRollback();
        TransferRollback Rollback { get; set; }// initialised on transaction start

        protected override void OnBeforeStart()
        {
            InitSlotDelegates();

            var actionButtons = GetComponentsInChildren<UIActionButton>();
            foreach (var button in actionButtons)
                button.OnActionButtonClick += OnActionButtonPressed;

            library = libManager.Library;
            InitLibraryHeroSlots();

            ShowHeroeLibraryCards();
        }

        private void ShowHeroeLibraryCards()
        {
            foreach (var hero in library.Heroes)
                libraryHeroSlots[hero.Key].Hero = hero.Value;
        }

        private void InitLibraryHeroSlots()
        {
            libraryHeroSlots = libraryContainer.GetComponentsInChildren<LibrarySlot>();
            for (int i = 0; i < libraryHeroSlots.Length; i++)
            {
                var slot = libraryHeroSlots[i];
                slot.SlotIndex = i;
                slot.DelegateProvider = slotDelegate;
                librarySlots[i] = slot;
            }
        }
        private void SyncHeroCardSelectionWithHero()
        {
            foreach (var card in librarySlots.Select(x => x.HeroCard).ToArray())
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
                    }
                    break;
                case Actions.ReloadMetadata:
                    {
                        libManager.LoadData();
                        ShowHeroeLibraryCards();
                    }
                    break;
                default:
                    break;
            }

        }


    }
}