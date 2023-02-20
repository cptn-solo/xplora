using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Assets.Scripts.Services
{    
    public partial class WorldService : MonoBehaviour
    {

        [SerializeField] private int width = 50;
        [SerializeField] private int height = 50;

        public int CellCount => width * height;

        private WorldState worldState = WorldState.NA;

        public WorldState WorldState => worldState;

        public event UnityAction OnTerrainProduced;
        public event UnityAction<int, bool> OnCellVisibilityChanged;


        internal void GenerateTerrain(CellProducerCallback cellCallback)
        {
            worldState = WorldState.TerrainBeingGenerated;

            TerrainProducer?.Invoke(width, height,
                cellCallback,
                () => { 
                    worldState = WorldState.SceneReady;
                    // DeployEcsWorldPoi(); // now relies on visibility
                    OnTerrainProduced?.Invoke();
                });
        }

        internal void CellVisibilityUpdated(int cellId, IVisibility visibilityRef, bool visible)
        {
            if (WorldState != Scripts.Data.WorldState.SceneReady)
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

            worldState = WorldState.NA;
        }

        /// <summary>
        /// Called from ecs to actually deploy POI
        /// </summary>
        /// <param name="cellId"></param>
        /// <returns></returns>
        internal POI POIDeploymentCallback(int cellId)
        {
            var coord = CellCoordinatesResolver(cellId);
            var pos = WorldPositionResolver(coord);

            var poi = PoiSpawner?.Invoke(pos, null);

            return poi;
        }

        internal void PoiDestroyCallback(POI poi)
        {
            // NB: return to pool maybe
            if (worldState == WorldState.SceneReady)
                GameObject.Destroy(poi.gameObject);
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
            worldRunloopCoroutine ??= StartCoroutine(WorldStateLoopCoroutine());
        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens previous, Screens current)
        {
            if (current == Screens.Raid)
                worldState = WorldState.AwaitingTerrain;

            if (previous == Screens.Raid)
                worldState = WorldState.AwaitingTerrainDestruction;
        }

        private void MenuNavigationService_OnNavigationToScreenComplete(
            Screens current)
        {
        }

        private void OnDestroy()
        {
            StopEcsWorldContext();
            if (worldRunloopCoroutine != null)
                StopCoroutine(worldRunloopCoroutine);

            OnDataAvailable -= WorldService_OnDataAvailable;

        }
    }
}