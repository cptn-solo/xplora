using System;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World;

namespace Assets.Scripts.ECS.Data
{
    public struct PlayerComp
    {
        public Hero Hero { get; internal set; } // avatar hero
        public int CellIndex { get; internal set; }
    }

    public struct OpponentComp
    {
        public Hero Hero { get; internal set; } // avatar hero
        public int CellIndex { get; internal set; }
    }

    /// <summary>
    /// Representation of non-static actor on the field (player/enemy teams)
    /// </summary>
    public struct UnitComp
    {
        public Unit Unit { get; internal set; }
    }

    public struct HeroComp { } // temp: reference to a hero data struct
    public struct TeamComp { } // temp: reference to a team of heroes
    public struct StaminaComp { } // can be filled and drained
    public struct PowerSourceComp { }
    public struct PowerComp { } // value based on stamina
    public struct FieldCellComp { } // cell on the world's map
    public struct POIComp { } // any static non-player object on the field
    public struct HostileComp { } // hostile unit/poi
    public struct FriendlyComp { } // friendly unit/poi
    public struct NeutralComp { } // neutral unit/poi

    public struct RefComponent
    {
    }
}


