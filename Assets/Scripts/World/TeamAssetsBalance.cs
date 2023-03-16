using Assets.Scripts.Data;
using Assets.Scripts.ECS;
using Assets.Scripts.Services;
using Zenject;

namespace Assets.Scripts.World
{
    public partial class TeamAssetsBalance : BaseEntityView<Team>
    {
        private readonly RaidService raidService;

        [Inject]
        public void Construct(RaidService raidService) =>
            raidService.RegisterEntityView<Team>(this, raidService.RaidEntity);            

    }
}