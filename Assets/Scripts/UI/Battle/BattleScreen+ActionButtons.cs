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
                if (button.Action == Actions.BeginBattle) // and start button too
                    button.SetEnabled(
                        battleManager.CanStartBattle ||
                        battleManager.CanMakeTurn);

                if (button.Action == Actions.StepBattle)
                    button.SetEnabled(
                        battleManager.CanStepPlayBattle);

                if (button.Action == Actions.AutoBattle)
                    button.SetEnabled(
                        battleManager.CanAutoPlayBattle);

                if (button.Action == Actions.RetreatBattle)
                    button.SetEnabled(
                        true);
            }
        }

        private void Button_OnActionButtonClick(Actions arg1, Transform arg2)
        {
            switch (arg1)
            {
                case Actions.ToggleLogPanel:
                    {
                        battleLog.TogglePanelSize();
                        arg2.GetComponent<UIActionToggleButton>().Toggle();
                    }
                    break;
                case Actions.BeginBattle:
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

