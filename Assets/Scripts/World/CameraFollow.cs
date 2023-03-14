using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public class CameraFollow : MonoBehaviour
    {
        [Inject] private readonly RaidService raidService = default;

        private Transform target;

        [SerializeField] private float distance = 5f;
        [SerializeField] private float angleX = 30f;
        [SerializeField] private float angleY = 0f;

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

            var lookDir = Quaternion.Euler(angleX, angleY, 0) * target.forward;
            var pos = target.position - lookDir * distance;
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }
}