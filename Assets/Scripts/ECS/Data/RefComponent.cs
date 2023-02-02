﻿using System;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Data
{
    public struct WorldComp { }
    public struct ProduceTag { }
    public struct DestroyTag { }

    public struct RaidComp
    {
        public Hero[] InitialPlayerHeroes { get; internal set; }
        public Hero[] InitialOpponentHeroes { get; internal set; }
    }

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


