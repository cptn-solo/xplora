using System;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Data
{
    #region Tags

    public struct DraftTag { }
    public struct ProduceTag { }
    public struct UpdateTag { }
    public struct RetireTag { }
    public struct DestroyTag { }
    public struct GarbageTag { }

    public struct WorldPoiTag { } // to separate world (static) poi from raid poi

    #endregion

    #region Refs
    public struct UnitRef
    {
        public Unit Unit { get; internal set; }
    }

    public struct PoiRef
    {
        public POI Poi { get; internal set; }
    }

    public struct UnitOverlayRef
    {
        public UnitOverlay Overlay;
    }

    public struct PackedEntityRef
    {
        public EcsPackedEntityWithWorld PackedEntity { get; internal set; }
    }

    public struct FieldVisibilityRef
    {
        public IVisibility visibility;
    }


    #endregion


    #region Comps
    public struct WorldComp
    {
        public EcsPackedEntity[] CellPackedEntities;
        public int PowerSourceCount { get; internal set; }
    }

    public struct RaidComp
    {
        public Hero[] InitialPlayerHeroes { get; internal set; }
        public Hero[] InitialOpponentHeroes { get; internal set; }
    }

    public struct BattleComp
    {
        public EcsPackedEntity EnemyPackedEntity { get; internal set; }
    }

    public struct BattleAftermathComp
    {
        public bool Won { get; internal set; }
    }

    public struct PlayerComp
    {
    }

    public struct OpponentComp
    {
    }

    public struct HeroComp
    {
        public Hero Hero { get; internal set; }
    }
    public struct TeamComp { } // temp: reference to a team of heroes
    public struct StaminaComp { } // can be filled and drained
    public struct SightRangeComp {
        public int Range { get; internal set; }
    }
    public struct PowerSourceComp { } // 
    public struct SpringComp { } // kind of powersource

    public struct VisitCellComp {
        public int CellIndex { get; internal set; }
    } // triggers systems aware of player being visiting a cell

    public struct RefillComp
    {
        public int Value { get; internal set; }
    }

    public struct DrainComp
    {
        public int Value { get; internal set; }
    }

    public struct FieldCellComp // cell on the world's map
    {
        public int CellIndex { get; internal set; }
    }

    public struct POIComp // any non-player object on the field
    {
    }

    public struct HostileComp { } // hostile unit/poi
    public struct FriendlyComp { } // friendly unit/poi
    public struct NeutralComp { } // neutral unit/poi
   

    #endregion

    public struct RefComponent
    {
    }
}


