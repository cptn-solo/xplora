using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Services
{
    public partial class WorldService : MonoBehaviour
    {
        [SerializeField] private int width = 6;
        [SerializeField] private int height = 6;

        public int CellCount => width * height;

        private WorldState worldState = WorldState.NA;

        public WorldState WorldState => worldState;

        public event UnityAction OnTerrainProduced;

        internal void GenerateTerrain()
        {
            worldState = WorldState.TerrainBeingGenerated;

            TerrainProducer?.Invoke(width, height, () => { 
                worldState = WorldState.SceneReady;
                DeployEcsWorldPoi();
                OnTerrainProduced?.Invoke();
            });
        }

        internal void DestroyTerrain()
        {
            worldState = WorldState.TerrainBeingDestoyed;
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
            GameObject.Destroy(poi);
        }


        internal void Init(
            MenuNavigationService menuNavigationService)
        {
            menuNavigationService.OnBeforeNavigateToScreen += MenuNavigationService_OnBeforeNavigateToScreen;
            menuNavigationService.OnNavigationToScreenComplete += MenuNavigationService_OnNavigationToScreenComplete;

            StartEcsWorldContext();
            worldRunloopCoroutine ??= StartCoroutine(WorldStateLoopCoroutine());                            
        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens previous, Screens current)
        {
            if (current == Screens.Raid)
                worldState = WorldState.AwaitingTerrain;

            if (previous == Screens.Raid)
            {
                DestroyEcsWorldPoi();
                worldState = WorldState.NA;
            }
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

        }
    }
}