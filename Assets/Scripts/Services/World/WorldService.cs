using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Services
{
    public partial class WorldService : BaseEcsService
    {

        [SerializeField] private int width = 50;
        [SerializeField] private int height = 50;

        public int CellCount => width * height;
        public int WorldWidth => width;
        public int WorldHeight => height;
       
        public WorldState WorldState { get; set; } = WorldState.NA;

        public event UnityAction OnTerrainProduced;
        public event UnityAction<int, bool> OnCellVisibilityChanged;


        internal void GenerateTerrain(CellProducerCallback cellCallback)
        {
            WorldState = WorldState.TerrainBeingGenerated;

            TerrainProducer?.Invoke(width, height,
                cellCallback,
                () => {
                    WorldState = WorldState.SceneReady;
                    // DeployEcsWorldPoi(); // now relies on visibility
                    OnTerrainProduced?.Invoke();
                });
        }

        internal void CellVisibilityUpdated(int cellId, IVisibility visibilityRef, bool visible)
        {
            if (WorldState != WorldState.SceneReady)
                return;

            if (visible)
                visibilityRef.IncreaseVisibility();
            else
                visibilityRef.DecreaseVisibility();

            OnCellVisibilityChanged?.Invoke(cellId, visible);
        }

        internal void DestroyTerrain()
        {
            DestroyEcsWorldPoi();

            WorldState = WorldState.NA;
        }

        internal void Init(
            MenuNavigationService menuNavigationService)
        {
            menuNavigationService.OnBeforeNavigateToScreen += MenuNavigationService_OnBeforeNavigateToScreen;
            menuNavigationService.OnNavigationToScreenComplete += MenuNavigationService_OnNavigationToScreenComplete;

            InitConfigLoading();

            OnDataAvailable += WorldService_OnDataAvailable;
        }

        private void WorldService_OnDataAvailable()
        {
            StartEcsWorldContext();
            runloopCoroutine ??= StartCoroutine(RunloopCoroutine());
        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens previous, Screens current)
        {
            if (current == Screens.Raid)
                WorldState = WorldState.AwaitingTerrain;

            if (previous == Screens.Raid)
                WorldState = WorldState.AwaitingTerrainDestruction;
        }

        private void MenuNavigationService_OnNavigationToScreenComplete(
            Screens current)
        {
        }

        private void OnDestroy()
        {
            StopRunloopCoroutine();
            StopEcsWorldContext();

            OnDataAvailable -= WorldService_OnDataAvailable;

        }
    }
}