using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class ProxyUnitsWorld : MonoBehaviour
    {
        [Inject] private readonly WorldService worldService;

        [SerializeField] private GameObject unitPrefab;

        private CameraFollow cameraFollow;

        private void Awake()
        {
            cameraFollow = GetComponent<CameraFollow>();
        }
        private void Start()
        {
            worldService.UnitSpawner = UnitSpawner;
            worldService.OnUnitSpawned += WorldService_OnUnitSpawned;
        }

        private void OnDestroy()
        {
            worldService.OnUnitSpawned -= WorldService_OnUnitSpawned;
        }

        private void WorldService_OnUnitSpawned(Unit unit, bool isPlayer)
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