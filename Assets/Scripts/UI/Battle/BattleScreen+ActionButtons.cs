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
                    button.gameObject.SetActive(battleManager.CanPrepareRound);

                if (button.Action == Actions.BeginBattle)
                    button.gameObject.SetActive(battleManager.CanBeginBattle);

                if (button.Action == Actions.CompleteTurn)
                    button.gameObject.SetActive(battleManager.CanMakeTurn);

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

                        battleManager.PrepareRound();

                        UpdateActionButtons();
                    }
                    break;
                case Actions.CompleteTurn:
                    {
                        battleManager.CompleteTurn();

                        UpdateActionButtons();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

