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
            worldService.CoordBeforeSelector = WorldCoordBeforeSelect;
            this.worldService = worldService;
            this.menuNavigationService = menuNavigationService;
            this.libManagementService = libManagementService;

            State = RaidState.NA;
        }

        private void WorldService_OnTerrainProduced()
        {
            State = RaidState.AwaitingUnits;

            DeployEcsWorldUnits();
        }

        internal void OnUnitsSpawned()
        {
            State = RaidState.UnitsSpawned;
        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens previous, Screens current)
        {
            if (current == Screens.Raid)
                StartEcsRaidContext();

            if (previous == Screens.Raid)
            {
                worldService.PlayerUnit = null;
                State = RaidState.NA;

                if (current != Screens.Battle)
                    MarkEcsWorldRaidForTeardown();
                else
                    State = RaidState.InBattle;
            }

        }

        private void MenuNavigationService_OnNavigationToScreenComplete(
            Screens current)
        {
        }

        /// <summary>
        /// Called from ecs during initialization
        /// </summary>
        /// <param name="playerHeroes"></param>
        /// <param name="opponentHeroes"></param>
        /// <returns></returns>
        public bool AssignPlayerAndEnemies(
            out Hero[] playerHeroes,
            out Hero[] opponentHeroes)
        {
            var library = libManagementService.Library;
            playerHeroes = library.PlayerTeamHeroes();
            opponentHeroes = library.NonPlayerTeamHeroes();

            return playerHeroes.Length > 0;
        }

        /// <summary>
        /// Called from ecs to actually deploy unit being registered with
        /// the opponent
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="hero"></param>
        /// <returns></returns>
        internal Unit PlayerDeploymentCallback(int cellId, Hero hero)
        {
            var coord = worldService.CellCoordinatesResolver(cellId);
            var pos = worldService.WorldPositionResolver(coord);

            var playerUnit = UnitSpawner?.Invoke(pos, hero, null);
            playerUnit.SetInitialCoordinates(coord);

            OnUnitSpawned?.Invoke(playerUnit, true);

            playerUnit.OnArrivedToCoordinates += PlayerUnit_OnArrivedToCoordinates;

            worldService.PlayerUnit = playerUnit;

            // temp: to mark cell as visited. Need to be implemented via ecs
            worldService.CoordSelector?.Invoke(coord);

            return playerUnit;

        }

        /// <summary>
        /// Called from ecs to actually deploy unit being registered with
        /// the opponent
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="hero"></param>
        /// <returns></returns>
        internal Unit OpponentDeploymentCallback(int cellId, Hero hero)
        {
            var coord = worldService.CellCoordinatesResolver(cellId);
            var pos = worldService.WorldPositionResolver(coord);

            var enemyUnit = UnitSpawner?.Invoke(pos, hero, null);

            OnUnitSpawned?.Invoke(enemyUnit, false);

            return enemyUnit;

        }

        internal void UnitDestroyCallback(Unit unit)
        {
            if (unit == worldService.PlayerUnit)
            {
                worldService.PlayerUnit = null;
                Debug.Log("UnitDestroyCallback player unit destroyed");
            }
        }

        private void WorldCoordBeforeSelect(
            HexCoordinates? coordinates,
            HexCoordAccessorCallback callback = null)
        {
            if (!coordinates.HasValue)
                return;

            if (CheckEcsWorldForOpponent(
                worldService.CellIndexResolver(coordinates.Value),
                out var hero,
                out var packedEntity))
            {
                InitiateEcsWorldBattle(packedEntity);
            }
            else callback?.Invoke();
        }

        private void PlayerUnit_OnArrivedToCoordinates(
            HexCoordinates coordinates,
            Unit unit)
        {
            var cellId = worldService.CellIndexResolver(coordinates);

            worldService.SetAimToCoordinates(null);

            UpdateEcsPlayerCellId(cellId);
            worldService.SetAimByHexDir(); // will try to continue move to direction set earlier
        }

        private void OnDestroy()
        {
            StopEcsRaidContext();
        }
    }

}

