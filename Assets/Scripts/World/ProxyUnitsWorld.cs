using Assets.Scripts.Services;
using Assets.Scripts.UI.Battle;
using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Library;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using static UnityEngine.UI.CanvasScaler;

namespace Assets.Scripts.World
{
    public class ProxyUnitsWorld : MonoBehaviour
    {
        [Inject] private readonly RaidService raidService;

        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private GameObject overlayPrefab;
        [SerializeField] private Transform overlayParent;

        private Canvas canvas;

        private void Start()
        {
            raidService.UnitSpawner = UnitSpawner;
            raidService.UnitOverlaySpawner = UnitOverlaySpawner;
            canvas = GetComponentInParent<Canvas>();
        }

        private void OnDestroy()
        {
            raidService.UnitSpawner = null;
        }

        private Unit UnitSpawner(Vector3 pos, Hero hero, UnitSpawnerCallback onSpawned) {
            var unit = Instantiate(unitPrefab, pos, Quaternion.identity, transform).GetComponent<Unit>();
            
            unit.SetHero(hero);

            onSpawned?.Invoke();

            return unit;
        }

        private UnitOverlay UnitOverlaySpawner(Transform anchor)
        {
            UnitOverlay overlay = Instantiate(overlayPrefab).GetComponent<UnitOverlay>();
            overlay.transform.localScale = canvas.transform.localScale;
            overlay.transform.SetParent(overlayParent);
            overlay.transform.localRotation = Quaternion.identity;
            overlay.transform.localPosition = Vector3.zero;
            overlay.Attach(anchor);

            return overlay;

        }

    }
}