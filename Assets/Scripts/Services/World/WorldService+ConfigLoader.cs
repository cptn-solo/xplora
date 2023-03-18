using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.Services;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Assets.Scripts.Services
{
    public partial class WorldService : IConfigLoaderService // Config loading
    {
        [Inject] private readonly StreamingAssetsLoaderService saLoader = default;

        private TerrainAttributesLibrary terrainAttributesLibrary = TerrainAttributesLibrary.EmptyLibrary();
        private TerrainAttributesConfigLoader terrainAttributesConfigLoader;

        public TerrainAttributesLibrary TerrainAttributesLibrary => terrainAttributesLibrary;

        private TerrainEventLibrary terrainEventsLibrary = TerrainEventLibrary.EmptyLibrary();
        private TerrainEventsConfigLoader terrainEventsConfigLoader;

        public TerrainEventLibrary TerrainEventsLibrary => terrainEventsLibrary;

        private TerrainPOILibrary terrainPOILibrary = TerrainPOILibrary.EmptyLibrary();
        private TerrainPOIsConfigLoader terrainPOIsConfigLoader;

        public TerrainPOILibrary TerrainPOILibrary => terrainPOILibrary;

        public event UnityAction OnDataAvailable;

        public bool DataAvailable =>
            terrainAttributesConfigLoader != null &&
            terrainEventsConfigLoader != null &&
            terrainPOIsConfigLoader != null &&

            terrainAttributesConfigLoader.DataAvailable &&
            terrainEventsConfigLoader.DataAvailable &&
            terrainPOIsConfigLoader.DataAvailable;

        public void InitConfigLoading()
        {
            terrainAttributesConfigLoader = new(terrainAttributesLibrary, NotifyIfAllDataAvailable);
            terrainEventsConfigLoader = new(terrainEventsLibrary, NotifyIfAllDataAvailable);
            terrainPOIsConfigLoader = new(terrainPOILibrary, NotifyIfAllDataAvailable);
        }

        public void NotifyIfAllDataAvailable()
        {
            if (DataAvailable)
                OnDataAvailable?.Invoke();
        }

        public void LoadCachedData()
        {
            saLoader.LoadData(terrainAttributesConfigLoader.ConfigFileName, terrainAttributesConfigLoader.ProcessSerializedString);
            saLoader.LoadData(terrainEventsConfigLoader.ConfigFileName, terrainEventsConfigLoader.ProcessSerializedString);
            saLoader.LoadData(terrainPOIsConfigLoader.ConfigFileName, terrainPOIsConfigLoader.ProcessSerializedString);
        }

        public void LoadRemoteData()
        {
            terrainAttributesConfigLoader.LoadGoogleData();
            terrainEventsConfigLoader.LoadGoogleData();
            terrainPOIsConfigLoader.LoadGoogleData();
        }

    }
}
