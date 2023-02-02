﻿using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;
using System;

namespace Assets.Scripts.Services
{
    public partial class RaidService // Battle
    {

        internal void ProcessAftermath(bool won)
        {
            ProcessEcsBattleAftermath(won);
        }

        internal void RetireHero(Hero hero) =>
            libManagementService.Library.RetireHero(hero);

        internal void MoveEnemyToFront(Hero hero) =>
            libManagementService.Library.MoveToEnemyFrontLine(hero);

        internal void StartBattle() =>
            menuNavigationService.NavigateToScreen(Screens.Battle);

        internal void FinalizeLostBattle()
        {
            MarkEcsWorldRaidForTeardown();
            menuNavigationService.NavigateToScreen(Screens.HeroesLibrary);
        }

        internal void FinalizeWonBattle() =>
            menuNavigationService.NavigateToScreen(Screens.Raid);
    }
}


