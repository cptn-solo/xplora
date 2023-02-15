using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Action Buttons
    {

        private void UpdateActionButtons()
        {
            foreach (var button in actionButtons)
            {
                if (button.Action == Actions.CompleteTurn) // and start button too
                    button.gameObject.SetActive(
                        battleManager.CanStartBattle ||
                        battleManager.CanMakeTurn);

                if (button.Action == Actions.StepBattle)
                    button.gameObject.SetActive(
                        battleManager.CanStepPlayBattle);

                if (button.Action == Actions.AutoBattle)
                    button.gameObject.SetActive(
                        battleManager.CanAutoPlayBattle);

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
                case Actions.CompleteTurn:
                    {
                        if (battleManager.CurrentBattle.State == BattleState.TeamsPrepared)
                            battleManager.BeginBattle();
                        else
                            battleManager.AutoPlay();

                        UpdateActionButtons();
                    }
                    break;

                case Actions.AutoBattle:
                    {
                        ResetTurnProcessingQueue();

                        battleManager.FastForwardPlay();

                        UpdateActionButtons();
                    }
                    break;
                case Actions.StepBattle:
                    {
                        battleManager.StepPlayBattle();

                        UpdateActionButtons();
                    }
                    break;
                case Actions.RetreatBattle:
                    {
                        battleManager.RetreatBattle();

                        UpdateActionButtons();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

