using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World.HexMap;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class UnitsWorldProxy : MonoBehaviour
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
            worldService.SetUnitSpawner(UnitSpawner);

            var playerUnit = worldService.SpawnPlayer();
            if (playerUnit != null)
                cameraFollow.Attach(playerUnit.transform);
        }

        private Unit UnitSpawner(Vector2 pos, Hero hero) {
            var unit = Instantiate(unitPrefab, pos, Quaternion.identity, transform).GetComponent<Unit>();
            unit.SetHero(hero);
            return unit;
        }

    }
}