using Leopotam.EcsLite;
using Assets.Scripts.Battle;

namespace Assets.Scripts.UI.Battle
{
    public partial class RaidMember : IHeroInstanceEntity// ECS
    {
        public EcsPackedEntityWithWorld? HeroInstanceEntity { get; set; }
    }
}