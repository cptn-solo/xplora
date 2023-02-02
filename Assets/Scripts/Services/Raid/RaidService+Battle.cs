using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;

namespace Assets.Scripts.Services
{
    public partial class RaidService // Battle
    {
        internal bool InitiateBattle(int cellId)
        {
            if (!CheckEcsWorldForOpponent(cellId, out var enemyHero))
                return false;

            State = RaidState.PrepareBattle;

            return true;
        }

        internal void ProcessAftermath(bool won)
        {
            State = RaidState.Aftermath;

            StartEcsRaidContext();

            ProcessEcsBattleAftermath(won);
        }

        internal void RetireHero(Hero hero) =>
            libManagementService.Library.RetireHero(hero);

        internal void MoveEnemyToFront(Hero hero) =>
            libManagementService.Library.MoveToEnemyFrontLine(hero);

        internal void StartBattle() =>
            menuNavigationService.NavigateToScreen(Screens.Battle);

        internal void FinalizeLostBattle() =>
            menuNavigationService.NavigateToScreen(Screens.HeroesLibrary);

        internal void FinalizeWonBattle() =>
            menuNavigationService.NavigateToScreen(Screens.Raid);
    }
}


