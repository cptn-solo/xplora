using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;
using System;

namespace Assets.Scripts.Services
{
    public partial class RaidService // Battle
    {
        internal void StartBattle() =>
            menuNavigationService.NavigateToScreen(Screens.Battle);

        internal void FinalizeWonBattle() =>
            menuNavigationService.NavigateToScreen(Screens.Raid);
    }
}


