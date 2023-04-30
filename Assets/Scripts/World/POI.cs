using Assets.Scripts.ECS;
using Assets.Scripts.World.HexMap;
using UnityEngine;

namespace Assets.Scripts.World
{
    public partial class POI : BaseEntityView<bool>
    {
        private PoiAnimation poiAnimation;
        private HexCoordinates coordinates;

        public HexCoordinates CurrentCoord => coordinates;

        [SerializeField] private Transform visual;

        protected override void OnBeforeAwake() =>
            poiAnimation = GetComponentInChildren<PoiAnimation>();

        public void SetInitialCoordinates(HexCoordinates initialCoordinates) =>
            coordinates = initialCoordinates;

        internal void Toggle(bool toggle) =>
            poiAnimation.SetActive(toggle);

        internal void SetupAnimator<T>() =>
            poiAnimation.SetRuntimeAnimator<T>();

    }
}