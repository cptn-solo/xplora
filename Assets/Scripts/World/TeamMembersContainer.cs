using Assets.Scripts.Data;
using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public partial class TeamMembersContainer : MonoBehaviour, ITransform<Team>
    {
        private RaidService raidService;

        public Transform Transform => transform;

        [Inject]
        public void Construct(RaidService raidService)
        {
            raidService.RegisterTransformRef<Team>(this);
            this.raidService = raidService;
        }

        public void OnGameObjectDestroy()
        {
            raidService.UnregisterTransformRef<Team>(this);
        }

        private void OnDestroy()
        {
            OnGameObjectDestroy();
        }
    }
}