using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Data
{
    public struct BattleComp
    {
        public EcsPackedEntity EnemyPackedEntity { get; internal set; }
    }
    public struct BattleRefComp
    {
        public EcsPackedEntity BattlePackedEntity { get; internal set; }
    }
    public struct HeroConfigRefComp
    {
        public EcsPackedEntityWithWorld HeroConfigPackedEntity { get; internal set; }
    }
    public struct HeroInstanceRefComp
    {
        public EcsPackedEntityWithWorld HeroInstancePackedEntity { get; internal set; }
    }
    public struct BattleTurnRefComp
    {
        public EcsPackedEntity TurnPackedEntity { get; internal set; }
    }
    public struct BattleRoundRefComp
    {
        public EcsPackedEntity RoundPackedEntity { get; internal set; }
    }

    /// <summary>
    /// Current HP
    /// </summary>
    public struct HPComp
    {
        public int HP { get; set; }
    }

    /// <summary>
    /// Initial HP
    /// </summary>
    public struct HealthComp
    {
        public int Health { get; set; }
    }
    public struct SpeedComp
    {
        public int Speed { get; set; }
    }
    public struct NameComp
    {
        public string Name { get; set; }
    }
    public struct PlayerTeamTag { }
    public struct EnemyTeamTag { }
    public struct DeadTag { }

    public struct FrontlineTag { }
    public struct BacklineTag { }

    public struct RangedTag { }




}


