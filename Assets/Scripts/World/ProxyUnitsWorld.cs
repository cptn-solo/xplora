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

        private void Start()
        {
            raidService.UnitSpawner = UnitSpawner;
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

    }
}