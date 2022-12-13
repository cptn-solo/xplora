using Assets.Scripts.UI.Data;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Action Buttons
    {

        private void UpdateActionButtons()
        {
            foreach (var button in actionButtons)
            {
                if (button.Action == Actions.PrepareQueue)
                    button.gameObject.SetActive(battleManager.CanReorderTurns);

                if (button.Action == Actions.CompleteTurn)
                    button.gameObject.SetActive(battleManager.CanMakeTurn);

                if (button.Action == Actions.AutoBattle)
                    button.gameObject.SetActive(battleManager.CanAutoPlayBattle);
            }
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
                case Actions.PrepareQueue:
                    {
                        if (inventoryToggle)
                            ToggleInventory();

                        battleManager.ResetBattle();

                        UpdateActionButtons();
                    }
                    break;
                case Actions.CompleteTurn:
                    {
                        battleManager.CompleteTurn();

                        UpdateActionButtons();
                    }
                    break;

                case Actions.AutoBattle:
                    {
                        ResetTurnProcessingQueue();

                        battleManager.Autoplay();

                        UpdateActionButtons();
                    }
                    break;
                case Actions.RetreatBattle:
                    {
                        navService.NavigateToScreen(Screens.HeroesLibrary);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

