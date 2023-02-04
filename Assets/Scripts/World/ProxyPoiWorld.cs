using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class ProxyPoiWorld : MonoBehaviour
    {
        [Inject] private readonly WorldService worldService;
        [SerializeField] private GameObject poiPrefab;

        private void Start()
        {
            worldService.PoiSpawner = PoiSpawner;
        }

        private void OnDestroy()
        {
            worldService.PoiSpawner = null;
        }

        private POI PoiSpawner(Vector3 pos, PoiSpawnerCallback onSpawned)
        {
            var poi = Instantiate(poiPrefab, pos, Quaternion.identity, transform).GetComponent<POI>();

            //unit.SetHero(hero);

            onSpawned?.Invoke();

            return poi;
        }

    }
}