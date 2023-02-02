using System;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Data
{
    public struct WorldComp { }
    public struct ProduceTag { }
    public struct DestroyTag { }
    public struct DraftTag { }

    public struct RaidComp
    {
        public Hero[] InitialPlayerHeroes { get; internal set; }
        public Hero[] InitialOpponentHeroes { get; internal set; }
    }

    public struct PlayerComp
    {
    }

    public struct OpponentComp
    {
    }

    /// <summary>
    /// Representation of non-static actor on the field (player/enemy teams)
    /// </summary>
    public struct UnitComp
    {
        public Unit Unit { get; internal set; }
    }

    public struct HeroComp
    {
        public Hero Hero { get; internal set; }
    }
    public struct TeamComp { } // temp: reference to a team of heroes
    public struct StaminaComp { } // can be filled and drained
    public struct PowerSourceComp { }
    public struct PowerComp { } // value based on stamina

    public struct FieldCellComp // cell on the world's map
    {
        public int CellIndex { get; internal set; }
    }

    public struct POIComp // any non-player object on the field
    {
        public EcsPackedEntityWithWorld PackedEntity { get; internal set; }
    }

    public struct HostileComp { } // hostile unit/poi
    public struct FriendlyComp { } // friendly unit/poi
    public struct NeutralComp { } // neutral unit/poi

    /// <summary>
    /// Marks battling entities
    /// </summary>
    public struct BattleComp
    {
        public EcsPackedEntity EnemyPackedEntity { get; internal set; }
    }

    public struct BattleAftermathComp
    {
        public bool Won { get; internal set; }
    }

    public struct RetireTag { }

    public struct RefComponent
    {
    }
}


