using Assets.Scripts.Data;
using Assets.Scripts.ECS;
using Assets.Scripts.Services;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.World
{
    public partial class TeamMembersContainer : BaseEntityViewContainer<RaidService, Team>
    {
    }
}