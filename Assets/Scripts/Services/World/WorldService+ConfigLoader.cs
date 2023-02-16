using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.Services.App;
using Assets.Scripts.Services.ConfigDataManagement.Parsers;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Assets.Scripts.Services
{
    public partial class WorldService //Config loading
    {
        [Inject] private readonly StreamingAssetsLoaderService saLoader;

        private TerrainAttributesLibrary terrainAttributesLibrary = TerrainAttributesLibrary.EmptyLibrary();
        private TerrainAttributesConfigLoader terrainAttributesConfigLoader;

        public TerrainAttributesLibrary TerrainAttributesLibrary => terrainAttributesLibrary;

        private TerrainEventLibrary terrainEventsLibrary = TerrainEventLibrary.EmptyLibrary();
        private TerrainEventsConfigLoader terrainEventsConfigLoader;

        public TerrainEventLibrary TerrainEventsLibrary => terrainEventsLibrary;

        public event UnityAction OnDataAvailable;

        public bool DataAvailable =>
            terrainAttributesConfigLoader != null &&
            terrainEventsConfigLoader != null &&
            terrainAttributesConfigLoader.DataAvailable &&
            terrainEventsConfigLoader.DataAvailable;

        private void InitConfigLoading()
        {
            terrainAttributesConfigLoader = new(terrainAttributesLibrary, NotifyIfAllDataAvailable);
            terrainEventsConfigLoader = new(terrainEventsLibrary, NotifyIfAllDataAvailable);
        }

        private void NotifyIfAllDataAvailable()
        {
            if (DataAvailable)
                OnDataAvailable?.Invoke();
        }

        public void LoadData()
        {
            saLoader.LoadData(terrainAttributesConfigLoader.ConfigFileName, terrainAttributesConfigLoader.ProcessSerializedString);
            saLoader.LoadData(terrainEventsConfigLoader.ConfigFileName, terrainEventsConfigLoader.ProcessSerializedString);
        }
    }
}
