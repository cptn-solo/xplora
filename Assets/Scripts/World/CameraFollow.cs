using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class CameraFollow : MonoBehaviour
    {
        [Inject] private readonly RaidService raidService;

        private Transform target;

        [SerializeField] private float distance = 5f;
        [SerializeField] private float angle = 30f;

        private void Start()
        {
            raidService.OnUnitSpawned += RaidService_OnUnitSpawned;
        }

        private void OnDestroy()
        {
            raidService.OnUnitSpawned -= RaidService_OnUnitSpawned;
        }

        private void RaidService_OnUnitSpawned(Unit unit, bool isPlayer)
        {
            if (unit != null && isPlayer)
                Attach(unit.transform);
        }


        public void Attach(Transform target)
        {
            this.target = target;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            var pos = target.position - transform.forward * distance;
            transform.position = pos;
        }
    }
}