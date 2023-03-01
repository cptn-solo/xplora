using UnityEngine;
using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;
using UnityEngine.Events;
using Assets.Scripts.World;
using Assets.Scripts.World.HexMap;
using System;

namespace Assets.Scripts.Services
{
    public partial class RaidService : BaseEcsService
    {
        private HeroLibraryService libManagementService;
        private BattleManagementService battleManagementService;
        private AudioPlaybackService audioService;
        private MenuNavigationService menuNavigationService;
        private WorldService worldService;

        public RaidState State { get; internal set; }

        public UnitSpawner UnitSpawner { get; internal set; }
        public UnitOverlaySpawner UnitOverlaySpawner { get; internal set; }

        public event UnityAction<Unit, bool> OnUnitSpawned;

        internal EntityViewFactory<TeamMemberInfo> TeamMemberFactory { get; set; } = null;

        public void Init(
            MenuNavigationService menuNavigationService,
            HeroLibraryService libManagementService,
            BattleManagementService battleManagementService,
            WorldService worldService,
            AudioPlaybackService audioService)
        {
            menuNavigationService.OnBeforeNavigateToScreen += MenuNavigationService_OnBeforeNavigateToScreen;
            menuNavigationService.OnNavigationToScreenComplete += MenuNavigationService_OnNavigationToScreenComplete;

            worldService.OnTerrainProduced += WorldService_OnTerrainProduced;
            worldService.OnCellVisibilityChanged += WorldService_OnCellVisibilityChanged;
            worldService.CoordBeforeSelector = WorldCoordBeforeSelect;

            this.worldService = worldService;
            this.menuNavigationService = menuNavigationService;
            this.libManagementService = libManagementService;
            this.battleManagementService = battleManagementService;
            this.audioService = audioService;

            battleManagementService.OnBattleComplete += BattleManagementService_OnBattleComplete;

            State = RaidState.NA;
        }

        private void BattleManagementService_OnBattleComplete(bool won)
        {
            ProcessEcsBattleAftermath(won);
        }

        private void WorldService_OnCellVisibilityChanged(int cellIndex, bool visible)
        {
            if (visible)
                DeployEcsWorldUnits(cellIndex);
            else
                DestroyEcsWorldUnits(cellIndex);
        }

        private void WorldService_OnTerrainProduced()
        {
            State = RaidState.AwaitingUnits;

            VisitEcsCellId();

            //DeployEcsWorldUnits();
        }

        internal void OnUnitsSpawned()
        {
            State = RaidState.UnitsSpawned;
        }

        internal void FinalizeRaid()
        {
            MarkEcsWorldRaidForTeardown();
        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens previous, Screens current)
        {
            if (current == Screens.Raid)
                StartEcsWorld();

            if (previous == Screens.Raid)
            {
                worldService.PlayerUnit = null;
                State = RaidState.NA;

                if (current == Screens.Battle)
                    State = RaidState.InBattle;
            }

        }

        private void MenuNavigationService_OnNavigationToScreenComplete(
            Screens current)
        {
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

            var playerUnit = UnitSpawner?.Invoke(pos, hero, true, null) ;
            playerUnit.SetInitialCoordinates(coord);

            OnUnitSpawned?.Invoke(playerUnit, true);

            playerUnit.OnArrivedToCoordinates += PlayerUnit_OnArrivedToCoordinates;

            worldService.PlayerUnit = playerUnit;

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

            var enemyUnit = UnitSpawner?.Invoke(pos, hero, false, null);

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

            if (worldService.WorldState == WorldState.SceneReady)
                GameObject.Destroy(unit.gameObject);
        }

        private void WorldCoordBeforeSelect(
            HexCoordinates? coordinates,
            HexCoordAccessorCallback callback = null)
        {
            if (!coordinates.HasValue)
                return;

            if (CheckEcsRaidForBattle())
                return;

            if (CheckEcsWorldForOpponent(
                worldService.CellIndexResolver(coordinates.Value),
                out var hero,
                out var packedEntity))
            {
                worldService.ResetHexDir();

                InitiateEcsWorldBattle(packedEntity);
            }
            else
            {
                var cellId = worldService.CellIndexResolver(coordinates.Value);

                worldService.SetAimToCoordinates(null);

                VisitEcsCellId(cellId);

                if (CheckEcsWorldForAttributes(cellId, out var attribute))
                    ProcessTerrainAttribute(attribute);

                callback?.Invoke();
            }
        }

        private void PlayerUnit_OnArrivedToCoordinates(
            HexCoordinates coordinates,
            Unit unit)
        {
            worldService.SetAimByHexDir(); // will try to continue move to direction set earlier
        }

        private void OnDestroy()
        {
            StopEcsWorld();
        }
    }

}

