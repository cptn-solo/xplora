using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class ProxyUnitsWorld : MonoBehaviour
    {
        [Inject] private readonly RaidService raidService;

        [SerializeField] private GameObject unitPrefab;

        private CameraFollow cameraFollow;

        private void Awake()
        {
            cameraFollow = GetComponent<CameraFollow>();
        }
        private void Start()
        {
            raidService.UnitSpawner = UnitSpawner;
            raidService.OnUnitSpawned += RaidService_OnUnitSpawned;
        }

        private void OnDestroy()
        {
            raidService.OnUnitSpawned -= RaidService_OnUnitSpawned;
            raidService.UnitSpawner = null;
        }

        private void RaidService_OnUnitSpawned(Unit unit, bool isPlayer)
        {
            if (unit != null && isPlayer)
                cameraFollow.Attach(unit.transform);
        }

        private Unit UnitSpawner(Vector3 pos, Hero hero, UnitSpawnerCallback onSpawned) {
            var unit = Instantiate(unitPrefab, pos, Quaternion.identity, transform).GetComponent<Unit>();
            
            unit.SetHero(hero);

            onSpawned?.Invoke();

            return unit;
        }

    }
}