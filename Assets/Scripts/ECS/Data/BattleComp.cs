using Assets.Scripts.Data;
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
    public interface IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld Packed { get; }
    }
    public struct HeroConfigRefComp : IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld HeroConfigPackedEntity { get; internal set; }
        public EcsPackedEntityWithWorld Packed => HeroConfigPackedEntity;
    }
    public struct HeroInstanceRefComp : IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld HeroInstancePackedEntity { get; internal set; }
        public EcsPackedEntityWithWorld Packed => HeroInstancePackedEntity;

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

    public struct BattleInProgressTag { }
    public struct BattleCompletedTag { }
    public struct RetreatTag { }
    public struct RoundInProgressTag { }
    public struct RoundShortageTag { }
    public struct PlayerTeamTag { }
    public struct EnemyTeamTag { }
    public struct DeadTag { }
    public struct RetiredTag { }
    public struct WinnerTag { }

    public struct ReadyTurnTag { } // Ready to make turn
    public struct MakeTurnTag { } // To activate prepared turn execution
    public struct CompletedTurnTag { } // Activate finalize
    public struct ProcessedTurnTag { } // Destroy

    public struct FrontlineTag { }
    public struct BacklineTag { }

    public struct RangedTag { } // hero
    public struct SkippedTag { } // turn
    public struct AttackerEffectsTag { } // turn
    public struct AttackTag { } // turn
    public struct DealDamageTag { } // turn
    public struct DealEffectsTag { } // turn

    public struct AttackerRef : IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld HeroInstancePackedEntity { get; set; }

        public EcsPackedEntityWithWorld Packed => HeroInstancePackedEntity;
    }
    public struct TargetRef : IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld HeroInstancePackedEntity { get; set; }

        public EcsPackedEntityWithWorld Packed => HeroInstancePackedEntity;
    }




}


