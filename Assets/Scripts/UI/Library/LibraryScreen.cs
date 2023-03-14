using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System;
using UnityEngine;
using Zenject;
using Assets.Scripts.Battle;
using System.Collections.Generic;

namespace Assets.Scripts.UI.Library
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public partial class LibraryScreen : MenuScreen
    {
        [Inject] private readonly BattleManagementService battleManager = default;
        [Inject] private readonly MenuNavigationService nav = default;

        private HeroLibraryService libManager;

        [Inject]
        public void Construct(HeroLibraryService libManager)
        {
            this.libManager = libManager;
            libManager.OnDataAvailable += LibManager_OnDataAvailable;

        }

        [SerializeField] private Transform libraryContainer;
        [SerializeField] private Transform playerTeamContainer;
        [SerializeField] private Transform enemyTeamContainer;
        
        [SerializeField] private HeroCardPool cardPool;

        [SerializeField] private UIMenuButton raidButton;

        private bool googleHeroesAvailable = true;

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

            cardPool.CardBinder = BindHeroCard;

            var slots = new Dictionary<HeroPosition, IHeroPosition>();

            InitSlots<LibrarySlot>(slots, libraryContainer, -1);
            InitSlots<PlayerTeamSlot>(slots, playerTeamContainer, 0);
            InitSlots<EnemyTeamSlot>(slots, enemyTeamContainer, 1);

            libManager.BindEcsHeroSlots(slots);            
        }

        private void LibManager_OnDataAvailable()
        {
            SyncWorldAndButton();
        }

        private void InitSlots<T>(Dictionary<HeroPosition, IHeroPosition> dict, Transform container, int teamId) where T: LibrarySlot
        {
            var slots = container.GetComponentsInChildren<T>();
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];

                var line = (slot is TeamMemberSlot) ? BattleLine.Front : BattleLine.NA;

                slot.Position = new HeroPosition(teamId, line, i); 
                slot.SlotIndex = i;
                slot.DelegateProvider = slotDelegate;
                dict.Add(slot.Position, slot);
            }
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

        protected override void OnBeforeDestroy()
        {
            libManager.DestroyEcsLibraryField();
            libManager.OnDataAvailable -= LibManager_OnDataAvailable;
        }

        private void OnActionButtonPressed(Actions action, Transform actionTransform)
        {
            switch (action)
            {
                case Actions.SelectHero:
                    {
                        var heroCard = actionTransform.GetComponent<HeroCard>();
                        Debug.Log($"Hero from line #{heroCard.Hero} selected");

                        libManager.SetEcsSelectedHero(heroCard.PackedEntity);
                    }
                    break;
                case Actions.SaveTeamForBattle:
                    {
                        battleManager.PlayerTeamPackedEntities =
                            libManager.WrapForBattle(libManager.PlayerHeroes);
                        battleManager.EnemyTeamPackedEntities =
                            libManager.WrapForBattle(libManager.EnemyHeroes);
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