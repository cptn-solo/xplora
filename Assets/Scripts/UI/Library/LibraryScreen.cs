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

        delegate void TransferRollback();
        TransferRollback Rollback { get; set; }// initialised on transaction start

        protected override void OnBeforeStart()
        {
            InitSlotDelegates();

            library = libManager.Library;

            var slots = libraryContainer.GetComponentsInChildren<LibrarySlot>();
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
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
        public GameObject ItemForHero(Hero hero)
        {
            HeroCard card = cardPool.GetHeroCard(hero);
            card.Hero = hero;

            if (hero.HeroType == HeroType.NA)
            {
                card.transform.localScale *= canvas.transform.localScale.x;
                BindHeroCard(card); //placeholders are just filled with data on cargo drop
            }

            return card.gameObject;
        }

        private void BindHeroCard(HeroCard heroCard)
        {
            var actionButton = heroCard.GetComponent<UIActionButton>();
            actionButton.OnActionButtonClick += HeroSelected;
        }


        private void HeroSelected(Actions action, Transform actionTransform)
        {
            var heroCard = actionTransform.GetComponent<HeroCard>();
            Debug.Log($"Hero from line #{heroCard.Hero} selected");

            selectedHero = heroCard.Hero;
            SyncHeroCardSelectionWithHero();
            //ShowHeroInventory(selectedHero);

        }


    }
}