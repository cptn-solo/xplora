using System;
using Assets.Scripts.World.HexMap;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class POI : MonoBehaviour
    {
        private PoiAnimation poiAnimation;
        private HexCoordinates coordinates;

        public HexCoordinates CurrentCoord => coordinates;

        [SerializeField] private Transform visual;

        private void Awake()
        {
            poiAnimation = GetComponentInChildren<PoiAnimation>();
        }

        public void SetInitialCoordinates(HexCoordinates initialCoordinates)
        {
            coordinates = initialCoordinates;
        }

        internal void Toggle(bool toggle)
        {
            poiAnimation.SetActive(toggle);
        }
    }
}