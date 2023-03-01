using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.World;
using Zenject;

namespace Assets.Scripts.UI.Inventory
{
    public class TeamMemberPool : BaseCardPool<TeamMember, TeamMemberInfo>
    {
        private RaidService raidService;

        [Inject]
        public void Construct(RaidService raidService)
        {
            this.raidService = raidService;
            this.raidService.TeamMemberFactory = CreateCard;
        }

        private void OnDestroy()
        {
            this.raidService.TeamMemberFactory = null;
        }


    }
}