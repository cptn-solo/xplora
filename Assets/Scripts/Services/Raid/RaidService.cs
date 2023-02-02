using UnityEngine;
using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;
using UnityEngine.Events;
using Assets.Scripts.World;
using Assets.Scripts.World.HexMap;

namespace Assets.Scripts.Services
{
    public partial class RaidService : MonoBehaviour
    {
        private HeroLibraryManagementService libManagementService;
        private MenuNavigationService menuNavigationService;
        private WorldService worldService;

        public RaidState State { get; internal set; }

        public Spawner UnitSpawner { get; internal set; }
        public event UnityAction<Unit, bool> OnUnitSpawned;

        public void Init(
            MenuNavigationService menuNavigationService,
            HeroLibraryManagementService libManagementService,
            WorldService worldService)
        {
            menuNavigationService.OnBeforeNavigateToScreen += MenuNavigationService_OnBeforeNavigateToScreen;
            menuNavigationService.OnNavigationToScreenComplete += MenuNavigationService_OnNavigationToScreenComplete;

            worldService.OnTerrainProduced += WorldService_OnTerrainProduced;

            this.worldService = worldService;
            this.menuNavigationService = menuNavigationService;
            this.libManagementService = libManagementService;
        }

        private void WorldService_OnTerrainProduced()
        {
            State = RaidState.AwaitingUnits;
        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens previous, Screens current)
        {
            if (current == Screens.Raid)
                StartEcsRaidContext();

            if (previous == Screens.Raid)
            {
                if (current == Screens.HeroesLibrary)
                    StopEcsRaidContext();
            }

        }

        private void MenuNavigationService_OnNavigationToScreenComplete(
            Screens current)
        {
            if (current == Screens.Battle)
                State = RaidState.InBattle;
        }

        public bool AssignPlayerAndEnemies(
            out Hero[] playerHeroes,
            out Hero[] opponentHeroes)
        {
            var library = libManagementService.Library;
            playerHeroes = library.PlayerTeamHeroes();
            opponentHeroes = library.NonPlayerTeamHeroes();

            return playerHeroes.Length > 0;
        }

        private void SpawnUnits()
        {
            State = RaidState.UnitsBeingSpawned;

            worldService.PlayerUnit = SpawnPlayer();

            SpawnEnemies(() => {
                State = RaidState.UnitsSpawned;
            });
        }

        private void DestroyUnits(DestroyUnitsCallback callback)
        {
            State = RaidState.UnitsBeingDestroyed;

            worldService.PlayerUnit = null;

            DestroyEcsUnits(callback);

        }

        private Unit PlayerDeploymentCallback(int cellId, Hero hero)
        {
            var coord = worldService.CellCoordinatesResolver(cellId);
            var pos = worldService.WorldPositionResolver(coord);

            var playerUnit = UnitSpawner?.Invoke(pos, hero, null);
            playerUnit.SetInitialCoordinates(coord);

            OnUnitSpawned?.Invoke(playerUnit, true);

            playerUnit.OnArrivedToCoordinates += PlayerUnit_OnArrivedToCoordinates;

            return playerUnit;

        }

        private Unit OpponentDeploymentCallback(int cellId, Hero hero)
        {
            var coord = worldService.CellCoordinatesResolver(cellId);
            var pos = worldService.WorldPositionResolver(coord);

            var enemyUnit = UnitSpawner?.Invoke(pos, hero, null);

            OnUnitSpawned?.Invoke(enemyUnit, false);

            return enemyUnit;

        }

        private Unit SpawnPlayer()
        {
            var playerUnit = DeployEcsWorldPlayer(PlayerDeploymentCallback);

            return playerUnit;
        }

        private void SpawnEnemies(UnitSpawnerCallback callback)
        {
            DeployEcsWorldOpponents(OpponentDeploymentCallback);

            callback?.Invoke();
        }

        private void PlayerUnit_OnArrivedToCoordinates(HexCoordinates coordinates, Unit unit)
        {
            var cellId = worldService.CellIndexResolver(coordinates);

            worldService.SetAimToCoordinates(null);

            if (!InitiateBattle(cellId))
            {
                UpdateEcsPlayerCellId(cellId);
                worldService.SetAimByHexDir(); // will try to continue move to direction set earlier
            }
        }

        private void OnDestroy()
        {
            StopEcsRaidContext();
        }


    }

}

