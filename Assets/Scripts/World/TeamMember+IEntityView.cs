using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.World
{
    public partial class TeamMember : IEntityView<TeamMemberInfo>
    {
        public override void UpdateData() =>
            Hero = DataLoader(PackedEntity);
    }
}