using UnityEngine;
using Assets.Scripts.UI.Data;
using Assets.Scripts.Data;
using UnityEngine.Events;
using Assets.Scripts.World;
using Assets.Scripts.World.HexMap;

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

        public event UnityAction<Unit, bool> OnUnitSpawned;

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

            InitConfigLoading();

            OnDataAvailable += RaidService_OnDataAvailable;
        }

        private void RaidService_OnDataAvailable()
        {
            // NB: raid is initialized upon scene navigation so no explicit reset is required
        }

        private void BattleManagementService_OnBattleComplete(bool won, Asset[] pot)
        {
            ProcessEcsBattleAftermath(won, pot);
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
            if (current == Screens.Raid &&
                opponentSpawnConfigLoader.DataAvailable &&
                teamSpawnConfigLoader.DataAvailable)
                StartEcsWorld();

            if (previous == Screens.Raid)
            {
                worldService.PlayerUnit = null;
                State = RaidState.NA;

                if (current == Screens.Battle)
                    State = RaidState.InBattle;
                else FinalizeRaid();
            }

        }

        private void MenuNavigationService_OnNavigationToScreenComplete(
            Screens current)
        {
        }

        internal void PlayerDeploymentCallback(Unit playerUnit, int cellId)
        {
            var coord = worldService.CellCoordinatesResolver(cellId);
            var pos = worldService.WorldPositionResolver(coord);

            playerUnit.SetInitialCoordinates(coord);
            playerUnit.Transform.SetPositionAndRotation(pos, Quaternion.identity);

            OnUnitSpawned?.Invoke(playerUnit, true);

            playerUnit.OnArrivedToCoordinates += PlayerUnit_OnArrivedToCoordinates;

            worldService.PlayerUnit = playerUnit;
        }

        internal void OpponentDeploymentCallback(Unit enemyUnit, int cellId)
        {
            var coord = worldService.CellCoordinatesResolver(cellId);
            var pos = worldService.WorldPositionResolver(coord);

            enemyUnit.Transform.SetPositionAndRotation(pos, Quaternion.identity);
            OnUnitSpawned?.Invoke(enemyUnit, false);
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

