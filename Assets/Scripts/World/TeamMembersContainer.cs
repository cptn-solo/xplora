using Assets.Scripts.Data;
using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public partial class TeamMembersContainer : MonoBehaviour, ITransform
    {
        private RaidService raidService;

        public Transform Transform => transform;

        [Inject]
        public void Construct(RaidService raidService)
        {
            raidService.RegisterTransformRef<Team>(this);
            this.raidService = raidService;
        }

        private void OnDestroy()
        {
            raidService.UnregisterTransformRef<Team>(this);
        }
    }
}