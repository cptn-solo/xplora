using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Assets.Scripts.ECS.Data
{
    public interface IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld Packed { get; }
    }

    public struct BattleComp
    {
        public EcsPackedEntity EnemyPackedEntity { get; internal set; }
    }

    public struct BattleRef
    {
        public EcsPackedEntity BattlePackedEntity { get; internal set; }
    }

    public struct HeroConfigRef : IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld HeroConfigRefPackedEntity { get; internal set; }
        public EcsPackedEntityWithWorld HeroConfigPackedEntity { get; internal set; }
        public readonly EcsPackedEntityWithWorld Packed => HeroConfigPackedEntity;
        public readonly EcsPackedEntityWithWorld RefPacked => HeroConfigRefPackedEntity;
    }

    public struct HeroInstanceMapping
    {
        /// <summary>
        /// Origin world entity packed used as a key, so when needed we can get a battle world entity by this key
        /// </summary>
        public Dictionary<EcsPackedEntityWithWorld, EcsPackedEntityWithWorld> OriginToBattleMapping { get; internal set; }

        /// <summary>
        /// Battle world entity packed used as a key, so when needed we can get an origin world entity by this key
        /// </summary>
        public Dictionary<EcsPackedEntityWithWorld, EcsPackedEntityWithWorld> BattleToOriginMapping { get; internal set; }
    }

    public struct BattleTurnRef
    {
        public EcsPackedEntity TurnPackedEntity { get; internal set; }
    }
    public struct BattleRoundRef
    {
        public EcsPackedEntity RoundPackedEntity { get; internal set; }
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
    public struct FinalizedTurnTag { } // Stop changing round's queue
    public struct ProcessedHeroTag { } // To tell if a hero card can be safely dropped for dead

    public struct FrontlineTag { }
    public struct BacklineTag { }

    public struct RangedTag { } // hero
    public struct SkippedTag { } // turn
    public struct AttackerEffectsTag { } // turn
    public struct AttackTag { } // turn
    public struct DealDamageTag { } // turn
    public struct DealEffectsTag { } // turn
    public struct AimTargetTag { } // icon to highlight aim target (added to bundle icon host)
    public struct UsedFocusEntityTag { }
    
    /// <summary>
    /// To be attached to a battle hero instance to track back damage and such for the raid
    /// </summary>
    public struct HeroInstanceOriginRef : IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld HeroInstancePackedEntity { get; internal set; }
        public readonly EcsPackedEntityWithWorld Packed => HeroInstancePackedEntity;
    }
    
    public struct HeroInstanceRef : IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld HeroInstancePackedEntity { get; internal set; }
        public readonly EcsPackedEntityWithWorld Packed => HeroInstancePackedEntity;
    }

    public struct AttackerRef : IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld HeroInstancePackedEntity { get; set; }
        public readonly EcsPackedEntityWithWorld Packed => HeroInstancePackedEntity;
    }

    public struct TargetRef : IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld HeroInstancePackedEntity { get; set; }
        public readonly EcsPackedEntityWithWorld Packed => HeroInstancePackedEntity;
    }

    public struct EffectFocusComp { 
        public RelationEffectKey EffectKey { get; set; }
        // who will be attacked
        public EcsPackedEntityWithWorld Focused { get; internal set; }
        // when to expire the focus
        public int EndRound { get; internal set; }
        // hero for whom the focused entity will be the target
        public EcsPackedEntityWithWorld Actor { get; internal set; }
    }

    public struct PackedEntityRef<T> : IPackedWithWorldRef where T : struct
    {
        public EcsPackedEntityWithWorld PackedEntity { get; internal set; }
        public readonly EcsPackedEntityWithWorld Packed => PackedEntity;
    }
}


